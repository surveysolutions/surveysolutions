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
                for (var i = 0; i < input.length; i++ , offset += 2) {
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
            currCallback

        this.node.onaudioprocess = function (e) {
            if (!recording) return
            worker.postMessage({
                command: 'record',
                buffer: [
                    e.inputBuffer.getChannelData(0),
                    e.inputBuffer.getChannelData(1),
                ],
            })
        }

        this.configure = function (cfg) {
            for (var prop in cfg) {
                if (Object.prototype.hasOwnProperty.call(cfg, prop)) {
                    config[prop] = cfg[prop]
                }
            }
        }

        this.record = function () {
            recording = true
        }

        this.stop = function () {
            recording = false
        }

        this.clear = function () {
            worker.postMessage({ command: 'clear' })
        }

        this.getBuffers = function (cb) {
            currCallback = cb || config.callback
            worker.postMessage({ command: 'getBuffers' })
        }

        this.exportWAV = function (cb, type) {
            currCallback = cb || config.callback
            type = type || config.type || 'audio/wav'
            if (!currCallback) throw new Error('Callback not set')
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

        var config = {}

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

            recorder = new window.Recorder(inputPoint)

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
            audio.onloadedmetadata = function(){
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
            config = configuration
            if (!navigator.getUserMedia)
                navigator.getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia
            if (!navigator.cancelAnimationFrame)
                navigator.cancelAnimationFrame = navigator.webkitCancelAnimationFrame || navigator.mozCancelAnimationFrame
            if (!navigator.requestAnimationFrame)
                navigator.requestAnimationFrame = navigator.webkitRequestAnimationFrame || navigator.mozRequestAnimationFrame

            if (!navigator.getUserMedia) {
                config.errorCallback()
            }
            else {
                navigator.getUserMedia(settings, successCallback, config.errorCallback)
            }
        }

        return self
    }


    window.AudioRecorder = AudioRecorder
}
