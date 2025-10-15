(function (window) {
    var Recorder = function (source, cfg) {
        var config = cfg || {}
        var bufferLen = config.bufferLen || 4096
        this.context = source.context
        if (!this.context.createScriptProcessor) {
            this.node = this.context.createJavaScriptNode(bufferLen, 2, 2)
        } else {
            this.node = this.context.createScriptProcessor(bufferLen, 2, 2)
        }

        // Check if WebCodecs API is supported
        this.supportsWebCodecs = typeof window.AudioEncoder !== 'undefined' &&
            typeof window.EncodedAudioChunk !== 'undefined';
        this.audioEncoder = null;
        this.encodedChunks = [];
        this.encoderConfig = config.encoderConfig || {
            codec: 'aac',  // Default codec, can also be 'opus'
            bitrate: 128000,
            numberOfChannels: 2,
            sampleRate: this.context.sampleRate
        };

        function workerWrapper() {
            var recLength = 0,
                recBuffersL = [],
                recBuffersR = [],
                sampleRate

            this.onmessage = function (e) {
                switch (e.data.command) {
                    case 'init':
                        init(e.data.config)
                        break
                    case 'record':
                        record(e.data.buffer)
                        break
                    case 'exportWAV':
                        exportWAV(e.data.type)
                        break
                    case 'getBuffers':
                        getBuffers()
                        break
                    case 'clear':
                        clear()
                        break
                }
            }

            function init(config) {
                sampleRate = config.sampleRate
            }

            function record(inputBuffer) {
                recBuffersL.push(inputBuffer[0])
                recBuffersR.push(inputBuffer[1])
                recLength += inputBuffer[0].length
            }

            function exportWAV(type) {
                var bufferL = mergeBuffers(recBuffersL, recLength)
                var bufferR = mergeBuffers(recBuffersR, recLength)
                var interleaved = interleave(bufferL, bufferR)
                var dataview = encodeWAV(interleaved)
                var audioBlob = new Blob([dataview], { type: type })

                this.postMessage(audioBlob)
            }

            function getBuffers() {
                var buffers = []
                buffers.push(mergeBuffers(recBuffersL, recLength))
                buffers.push(mergeBuffers(recBuffersR, recLength))
                this.postMessage(buffers)
            }

            function clear() {
                recLength = 0
                recBuffersL = []
                recBuffersR = []
            }

            function mergeBuffers(recBuffers, recLength) {
                var result = new Float32Array(recLength)
                var offset = 0
                for (var i = 0; i < recBuffers.length; i++) {
                    result.set(recBuffers[i], offset)
                    offset += recBuffers[i].length
                }
                return result
            }

            function interleave(inputL, inputR) {
                var length = inputL.length + inputR.length
                var result = new Float32Array(length)

                var index = 0,
                    inputIndex = 0

                while (index < length) {
                    result[index++] = inputL[inputIndex]
                    result[index++] = inputR[inputIndex]
                    inputIndex++
                }
                return result
            }

            function floatTo16BitPCM(output, offset, input) {
                for (var i = 0; i < input.length; i++, offset += 2) {
                    var s = Math.max(-1, Math.min(1, input[i]))
                    output.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7FFF, true)
                }
            }

            function writeString(view, offset, string) {
                for (var i = 0; i < string.length; i++) {
                    view.setUint8(offset + i, string.charCodeAt(i))
                }
            }

            function encodeWAV(samples, mono) {
                var buffer = new ArrayBuffer(44 + samples.length * 2)
                var view = new DataView(buffer)

                /* RIFF identifier */
                writeString(view, 0, 'RIFF')
                /* file length */
                view.setUint32(4, 32 + samples.length * 2, true)
                /* RIFF type */
                writeString(view, 8, 'WAVE')
                /* format chunk identifier */
                writeString(view, 12, 'fmt ')
                /* format chunk length */
                view.setUint32(16, 16, true)
                /* sample format (raw) */
                view.setUint16(20, 1, true)
                /* channel count */
                view.setUint16(22, mono ? 1 : 2, true)
                /* sample rate */
                view.setUint32(24, sampleRate, true)
                /* byte rate (sample rate * block align) */
                view.setUint32(28, sampleRate * 4, true)
                /* block align (channel count * bytes per sample) */
                view.setUint16(32, 4, true)
                /* bits per sample */
                view.setUint16(34, 16, true)
                /* data chunk identifier */
                writeString(view, 36, 'data')
                /* data chunk length */
                view.setUint32(40, samples.length * 2, true)

                floatTo16BitPCM(view, 44, samples)

                return view
            }
        }

        var code = workerWrapper.toString()
        code = code.substring(code.indexOf('{') + 1, code.lastIndexOf('}'))

        var workerBlob = new Blob([code], { type: 'application/javascript' })
        var worker = new Worker(URL.createObjectURL(workerBlob))

        worker.postMessage({
            command: 'init',
            config: {
                sampleRate: this.context.sampleRate,
            },
        })
        var recording = false,
            currCallback,
            encoding = false;

        this.node.onaudioprocess = function (e) {
            if (!recording) return

            var leftChannel = e.inputBuffer.getChannelData(0);
            var rightChannel = e.inputBuffer.getChannelData(1);

            worker.postMessage({
                command: 'record',
                buffer: [leftChannel, rightChannel],
            })

            // If WebCodecs is supported and we're encoding, feed the audio to the encoder
            if (this.supportsWebCodecs && this.audioEncoder && encoding) {
                this.encodeAudioData(leftChannel, rightChannel);
            }
        }.bind(this);

        this.configure = function (cfg) {
            for (var prop in cfg) {
                if (Object.prototype.hasOwnProperty.call(cfg, prop)) {
                    config[prop] = cfg[prop]
                }
            }

            // Configure encoder if WebCodecs settings are provided
            if (cfg.encoderConfig && this.supportsWebCodecs) {
                this.encoderConfig = Object.assign({}, this.encoderConfig, cfg.encoderConfig);
                if (this.audioEncoder) {
                    this.resetEncoder();
                }
            }
        }

        // Initialize the audio encoder
        this.initEncoder = function () {
            if (!this.supportsWebCodecs) return false;

            try {
                this.audioEncoder = new AudioEncoder({
                    output: chunk => {
                        this.encodedChunks.push(chunk);
                    },
                    error: err => {
                        console.error('AudioEncoder error:', err);
                    }
                });

                this.audioEncoder.configure({
                    codec: this.encoderConfig.codec,
                    bitrate: this.encoderConfig.bitrate,
                    numberOfChannels: this.encoderConfig.numberOfChannels,
                    sampleRate: this.encoderConfig.sampleRate
                });

                return true;
            } catch (error) {
                console.error('Failed to initialize AudioEncoder:', error);
                return false;
            }
        }

        // Reset the encoder and clear encoded chunks
        this.resetEncoder = function () {
            if (this.audioEncoder) {
                this.audioEncoder.reset();
                this.encodedChunks = [];
            }
        }

        // Encode audio data
        this.encodeAudioData = function (leftChannel, rightChannel) {
            if (!this.audioEncoder) return;

            try {
                const frameCount = leftChannel.length;
                // Create an interleaved buffer
                const audioData = new Float32Array(frameCount * 2);
                for (let i = 0; i < frameCount; i++) {
                    audioData[i * 2] = leftChannel[i];
                    audioData[i * 2 + 1] = rightChannel[i];
                }

                const frame = new AudioData({
                    format: 'f32',
                    sampleRate: this.context.sampleRate,
                    numberOfFrames: frameCount,
                    numberOfChannels: 2,
                    timestamp: performance.now() * 1000, // microseconds
                    data: audioData
                });

                this.audioEncoder.encode(frame);
                frame.close();
            } catch (error) {
                console.error('Failed to encode audio data:', error);
            }
        }

        this.record = function () {
            recording = true;

            // Initialize encoder if WebCodecs is supported
            if (this.supportsWebCodecs) {
                encoding = this.initEncoder();
            }
        }

        this.stop = function () {
            recording = false;
            encoding = false;
        }

        this.clear = function () {
            worker.postMessage({ command: 'clear' });

            // Reset encoder and clear encoded chunks
            if (this.supportsWebCodecs) {
                this.resetEncoder();
            }
        }

        // Export compressed audio
        this.exportCompressed = function (cb, mimeType) {
            currCallback = cb || config.callback;
            if (!currCallback) throw new Error('Callback not set');

            if (!this.supportsWebCodecs || this.encodedChunks.length === 0) {
                // Fall back to WAV if WebCodecs not supported or no encoded chunks
                return this.exportWAV(cb, 'audio/wav');
            }

            // Flush the encoder to get remaining encoded chunks
            this.audioEncoder.flush().then(() => {
                // Create a container based on the codec
                let containerType;
                let containerData;

                if (this.encoderConfig.codec === 'opus') {
                    containerType = 'audio/ogg; codecs=opus';
                    containerData = this.createOggOpusContainer();
                } else if (this.encoderConfig.codec === 'aac') {
                    containerType = 'audio/mp4; codecs=mp4a.40.2';
                    containerData = this.createMP4Container();
                } else {
                    // Default to raw format if codec isn't recognized
                    containerType = mimeType || 'audio/webm';
                    containerData = this.createRawContainer();
                }

                const blob = new Blob(containerData, { type: containerType });
                currCallback(blob);

                // Reset encoded chunks after export
                this.encodedChunks = [];
            });
        }

        // Create a simple container for the encoded data
        // Note: In a real implementation, you'd use a proper container format library
        this.createRawContainer = function () {
            const data = [];
            this.encodedChunks.forEach(chunk => {
                data.push(new Uint8Array(chunk.byteLength));
                data[data.length - 1].set(new Uint8Array(chunk.data));
            });
            return data;
        }

        // Create an OGG container for Opus audio
        this.createOggOpusContainer = function () {
            // Note: This is a placeholder. In practice, you would need a proper OGG muxer
            return this.createRawContainer();
        }

        // Create an MP4 container for AAC audio
        this.createMP4Container = function () {
            // Note: This is a placeholder. In practice, you would need a proper MP4 muxer
            return this.createRawContainer();
        }

        // Modify exportWAV to check for compressed audio option
        this.exportWAV = function (cb, type) {
            currCallback = cb || config.callback
            type = type || config.type || 'audio/wav'
            if (!currCallback) throw new Error('Callback not set')

            // Use compressed format if WebCodecs is supported and user hasn't opted out
            if (this.supportsWebCodecs && config.useCompression !== false && encoding) {
                return this.exportCompressed(cb, type);
            }

            worker.postMessage({
                command: 'exportWAV',
                type: type,
            })
        }

        worker.onmessage = function (e) {
            var blob = e.data
            currCallback(blob)
        }

        source.connect(this.node)
        this.node.connect(this.context.destination)   // if the script node is not connected to an output the "onaudioprocess" event is not triggered in chrome.
    }

    window.Recorder = Recorder

})(window)

if (!window.AudioRecorder) {
    var AudioRecorder = function () {
        var self = {}
        var recorder = null
        var analyserSettings = {
            сontext: null,
            node: null,
            canvasWidth: 0,
            canvasHeight: 0,
            animationFrameId: null,
        }

        var config = {
            useCompression: true,  // Enable WebCodecs compression by default
            encoderConfig: {
                codec: 'aac',     // Default codec
                bitrate: 128000    // Default bitrate
            }
        }

        var settings = {
            audio: {
                'mandatory': {
                    'googEchoCancellation': 'false',
                    'googAutoGainControl': 'false',
                    'googNoiseSuppression': 'false',
                    'googHighpassFilter': 'false',
                },
                'optional': [],
            },
        }

        var audioContext

        var successCallback = function (stream) {

            audioContext = new (window.AudioContext || window.webkitAudioContext)()
            var inputPoint = audioContext.createGain()

            // Create an AudioNode from the stream.
            var realAudioInput = audioContext.createMediaStreamSource(stream)
            var audioInput = realAudioInput
            audioInput.connect(inputPoint)

            analyserSettings.node = audioContext.createAnalyser()
            analyserSettings.node.fftSize = 2048
            inputPoint.connect(analyserSettings.node)

            recorder = new window.Recorder(inputPoint, {
                useCompression: config.useCompression,
                encoderConfig: config.encoderConfig
            });

            var zeroGain = audioContext.createGain()
            zeroGain.gain.value = 0.0
            inputPoint.connect(zeroGain)
            zeroGain.connect(audioContext.destination)
            analyserSettings.canvasWidth = config.analyserEl.width
            analyserSettings.canvasHeight = config.analyserEl.height
            analyserSettings.context = config.analyserEl.getContext('2d')

            updateAnalysers()
            self.start()
        }

        function gotBuffers() {
            recorder.exportWAV(doneEncoding)
        }

        function doneEncoding(blob) {
            var audioURL = window.URL.createObjectURL(blob)
            var audio = new Audio(audioURL)
            audio.onloadedmetadata = function () {
                var duration = audio.duration
                config.doneCallback(blob, duration)
            }
        }

        function cancelAnalyserUpdates() {
            window.cancelAnimationFrame(analyserSettings.animationFrameId)
            analyserSettings.animationFrameId = null
            if (audioContext) audioContext.close()
        }

        function updateAnalysers() {
            var freqByteData = new Uint8Array(analyserSettings.node.frequencyBinCount)

            var centerX = analyserSettings.canvasWidth / 2
            var centerY = analyserSettings.canvasHeight / 2

            analyserSettings.node.getByteFrequencyData(freqByteData)

            analyserSettings.context.clearRect(0, 0, analyserSettings.canvasWidth, analyserSettings.canvasHeight)
            var multiplier = analyserSettings.node.frequencyBinCount

            // Draw rectangle for each frequency bin.
            var magnitude = 0
            for (var j = 0; j < multiplier; j++)
                magnitude += freqByteData[j]
            magnitude = Math.min(magnitude / multiplier, analyserSettings.canvasWidth / 2)
            magnitude = Math.max(magnitude, 6)

            var color = ''
            if (magnitude < 10) {
                color = '#cbcbcb'
            } else if (magnitude > 42) {
                color = '#ed2c39'
            } else {
                color = '#2a81cb'
            }
            analyserSettings.context.strokeStyle = color
            analyserSettings.context.fillStyle = color
            analyserSettings.context.lineWidth = 2

            analyserSettings.context.beginPath()
            analyserSettings.context.arc(centerX, centerY, 4, 0, 2 * Math.PI, false)
            analyserSettings.context.fill()
            analyserSettings.context.stroke()

            analyserSettings.context.beginPath()
            analyserSettings.context.arc(centerX, centerY, magnitude, 0, 2 * Math.PI, false)
            analyserSettings.context.stroke()

            analyserSettings.animationFrameId = window.requestAnimationFrame(updateAnalysers)
        }

        self.start = function () {
            if (!recorder)
                return

            recorder.clear()
            recorder.record()
            config.startRecordingCallback()
        }

        self.stop = function () {
            if (!recorder)
                return

            recorder.stop()
            recorder.getBuffers(gotBuffers)

            cancelAnalyserUpdates()
        }

        self.cancel = function () {
            recorder.stop()
            cancelAnalyserUpdates()
            recorder.clear()
        }

        self.initAudio = function (configuration) {
            // Merge configurations
            for (var prop in configuration) {
                if (Object.prototype.hasOwnProperty.call(configuration, prop)) {
                    config[prop] = configuration[prop]
                }
            }

            // Check for MediaDevices API (newer API)
            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                navigator.mediaDevices.getUserMedia({ audio: settings.audio })
                    .then(successCallback)
                    .catch(config.errorCallback);
            }
            // Fallback to older API
            else if (!navigator.getUserMedia) {
                navigator.getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia;
                if (!navigator.getUserMedia) {
                    config.errorCallback();
                } else {
                    navigator.getUserMedia(settings, successCallback, config.errorCallback);
                }
            }
        }

        return self
    }

    window.AudioRecorder = AudioRecorder
}
