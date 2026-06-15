/**
 * Upload interop for Cloudflare Stream.
 * Supports TUS resumable uploads and XHR fallback for direct file upload.
 */
window.uploadInterop = {
    _dotNetRef: null,
    _currentUpload: null,

    /**
     * Initialize with a reference back to the Blazor component for callbacks.
     */
    initialize: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
        this._setupCameraInput();
    },

    /**
     * Listen for camera input changes and notify Blazor.
     */
    _setupCameraInput: function () {
        // Use MutationObserver to catch when the camera input is added to DOM
        const observer = new MutationObserver(() => {
            const cameraInput = document.getElementById('cameraFileInput');
            if (cameraInput && !cameraInput._listenerAttached) {
                cameraInput._listenerAttached = true;
                cameraInput.addEventListener('change', (e) => {
                    const file = e.target.files[0];
                    if (file && this._dotNetRef) {
                        this._dotNetRef.invokeMethodAsync('OnCameraFileSelected', file.name, file.size);
                    }
                });
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
    },

    /**
     * Programmatically trigger a file input click (used for camera capture).
     */
    triggerFileInput: function (inputId) {
        const input = document.getElementById(inputId);
        if (input) {
            input.click();
        }
    },

    /**
     * Get the file from either a specific input ID or from MudFileUpload's hidden input.
     */
    _getFile: function (inputId) {
        if (inputId) {
            const input = document.getElementById(inputId);
            return input?.files?.[0] || null;
        }

        // MudFileUpload renders a hidden <input type="file"> — find it
        const mudInputs = document.querySelectorAll('input[type="file"][accept="video/*"]');
        for (const input of mudInputs) {
            if (input.files && input.files.length > 0) {
                return input.files[0];
            }
        }
        return null;
    },

    /**
     * Upload a file to Cloudflare Stream using the TUS resumable upload protocol.
     * @param {string|null} inputId - Specific input element ID, or null to auto-detect from MudFileUpload
     * @param {string} uploadUrl - The Cloudflare Stream direct upload URL
     */
    tusUpload: function (inputId, uploadUrl) {
        const file = this._getFile(inputId);
        if (!file) {
            this._notifyError('No file selected. Please select a video file and try again.');
            return;
        }

        const self = this;

        // Use TUS protocol if library is available
        if (typeof tus !== 'undefined') {
            const upload = new tus.Upload(file, {
                endpoint: uploadUrl,
                uploadUrl: uploadUrl,
                retryDelays: [0, 1000, 3000, 5000],
                chunkSize: 50 * 1024 * 1024, // 50 MB chunks for large video files
                metadata: {
                    filename: file.name,
                    filetype: file.type || 'video/mp4'
                },
                onError: function (error) {
                    console.error('[uploadInterop] TUS error:', error);
                    self._notifyError(error.message || 'Upload failed. Please try again.');
                },
                onProgress: function (bytesUploaded, bytesTotal) {
                    const pct = Math.round((bytesUploaded / bytesTotal) * 100);
                    self._notifyProgress(pct);
                },
                onSuccess: function () {
                    self._notifyComplete();
                }
            });

            this._currentUpload = upload;
            upload.start();
        } else {
            // Fallback: simple FormData POST (Cloudflare direct upload also accepts this)
            this._formDataUpload(file, uploadUrl);
        }
    },

    /**
     * Upload from a cloud URL by POSTing to the Cloudflare upload endpoint.
     * @param {string} uploadUrl - The Cloudflare Stream direct upload URL
     * @param {string} sourceUrl - The cloud source URL to pull the video from
     */
    uploadFromUrl: function (uploadUrl, sourceUrl) {
        const self = this;
        const formData = new FormData();
        formData.append('url', sourceUrl);

        const xhr = new XMLHttpRequest();
        this._currentUpload = xhr;

        xhr.addEventListener('load', function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                self._notifyComplete();
            } else {
                self._notifyError('Failed to import from URL. Status: ' + xhr.status);
            }
        });

        xhr.addEventListener('error', function () {
            self._notifyError('Network error while importing from URL.');
        });

        xhr.open('POST', uploadUrl, true);
        xhr.send(formData);

        // Simulate progress for URL-based uploads (server-side pull)
        let progress = 0;
        const interval = setInterval(() => {
            if (progress < 90) {
                progress += 5;
                self._notifyProgress(progress);
            } else {
                clearInterval(interval);
            }
        }, 500);

        xhr.addEventListener('load', () => clearInterval(interval));
        xhr.addEventListener('error', () => clearInterval(interval));
    },

    /**
     * Fallback: Upload via FormData POST with progress tracking.
     */
    _formDataUpload: function (file, uploadUrl) {
        const self = this;
        const formData = new FormData();
        formData.append('file', file);

        const xhr = new XMLHttpRequest();
        this._currentUpload = xhr;

        xhr.upload.addEventListener('progress', function (e) {
            if (e.lengthComputable) {
                const pct = Math.round((e.loaded / e.total) * 100);
                self._notifyProgress(pct);
            }
        });

        xhr.addEventListener('load', function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                self._notifyComplete();
            } else {
                self._notifyError('Upload failed with status: ' + xhr.status);
            }
        });

        xhr.addEventListener('error', function () {
            self._notifyError('Network error during upload.');
        });

        xhr.addEventListener('abort', function () {
            self._notifyError('Upload was cancelled.');
        });

        xhr.open('POST', uploadUrl, true);
        xhr.send(formData);
    },

    /**
     * Cancel the current upload.
     */
    cancelUpload: function () {
        if (this._currentUpload) {
            if (typeof this._currentUpload.abort === 'function') {
                this._currentUpload.abort();
            }
            this._currentUpload = null;
        }
    },

    _notifyProgress: function (pct) {
        if (this._dotNetRef) {
            this._dotNetRef.invokeMethodAsync('OnUploadProgress', pct);
        }
    },

    _notifyComplete: function () {
        if (this._dotNetRef) {
            this._dotNetRef.invokeMethodAsync('OnUploadComplete');
        }
    },

    _notifyError: function (msg) {
        if (this._dotNetRef) {
            this._dotNetRef.invokeMethodAsync('OnUploadError', msg);
        }
    }
};
