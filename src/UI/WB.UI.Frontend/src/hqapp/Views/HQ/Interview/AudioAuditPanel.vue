<template>
    <aside id="audio-audit-panel" ref="panel" class="audio-audit-panel" tabindex="-1"
        aria-labelledby="audio-audit-panel-title" @keydown.esc="closePanel">
        <div class="audio-audit-header">
            <h4 id="audio-audit-panel-title">
                Audio audit records
            </h4>
            <button ref="closeButton" type="button" class="btn btn-link close-panel" @click="closePanel"
                :aria-label="$t('ReviewInterview.AudioAudit_CloseRecordings')">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>

        <div v-if="loadState === 'loading'" class="audio-audit-state" role="status">
            Loading recordings...
        </div>

        <div v-else-if="loadState === 'error'" class="alert alert-warning" role="alert">
            <p>Unable to load audio audit recordings.</p>
            <button type="button" class="btn btn-outline-secondary btn-sm" @click="loadSegments">
                Retry
            </button>
        </div>

        <div v-else-if="loadState === 'empty'" class="alert alert-info" role="status">
            {{ $t('ReviewInterview.AudioAudit_NoRecordingsAvailable') }}
        </div>

        <template v-else>
            <div v-if="currentSegment" class="audio-audit-player" :aria-busy="isBuffering">
                <audio ref="audioPlayer" :src="currentSegmentUrl" controlsList="nodownload" preload="auto"
                    style="display: none" @loadedmetadata="onMetadataLoaded" @canplay="onCanPlay" @play="onPlay"
                    @pause="onPause" @waiting="onWaiting" @playing="onPlaying" @error="onPlaybackError"
                    @timeupdate="onTimeUpdate" @ended="onEnded"></audio>

                <p class="sr-only" role="status" aria-live="polite">
                    {{ playbackAnnouncement }}
                </p>

                <div v-if="playbackError" class="alert alert-warning" role="alert">
                    <p>{{ playbackError }}</p>
                    <button v-if="!currentSegment.unavailable" type="button" class="btn btn-outline-secondary btn-sm"
                        @click="retryCurrentSegment">
                        Retry playback
                    </button>
                </div>

                <div class="player-controls">
                    <p class="now-playing">
                        Now playing: Record #{{ currentSegment.sequenceNumber }}
                    </p>
                    <div class="player-status">
                        <span>Position: {{ formatTime(currentTime) }}</span>
                        <span>Duration:
                            {{
                                currentDuration !== null
                                    ? formatTime(currentDuration)
                                    : '--:--'
                            }}</span>
                    </div>
                    <p class="player-buffering" :class="{ 'is-hidden': !isBuffering }" role="status"
                        :aria-hidden="!isBuffering">
                        Loading audio...
                    </p>
                    <label class="sr-only" for="audio-audit-seek">
                        Recording position
                    </label>
                    <input id="audio-audit-seek" type="range" class="seek-bar form-range" :max="currentDuration || 0"
                        :value="seekBarTime" :aria-valuemax="currentDuration || 0" :aria-valuenow="seekBarTime"
                        :aria-valuetext="seekAriaValue" @pointerdown="beginSeeking"
                        @input="previewSeek($event.target.value)" @change="commitSeek($event.target.value)"
                        :disabled="!canSeek" />
                    <div class="control-buttons">
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="goToPreviousSegment"
                            :disabled="!previousSegment" aria-label="Go to previous segment">
                            <span class="glyphicon glyphicon-step-backward" aria-hidden="true"></span>
                            <span class="sr-only">Go to previous segment</span>
                        </button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="skipBy(-10)"
                            :disabled="!canSeek" aria-label="Go back 10 seconds">
                            <span class="glyphicon glyphicon-backward" aria-hidden="true"></span>
                            <span class="sr-only">Go back 10 seconds</span>
                        </button>
                        <button type="button" class="btn btn-primary" @click="togglePlayPause"
                            :aria-label="playPauseLabel" :aria-pressed="isPlaying"
                            :disabled="currentSegment.unavailable">
                            <span class="glyphicon" :class="isPlaying
                                ? 'glyphicon-pause'
                                : 'glyphicon-play'
                                " aria-hidden="true"></span>
                            <span class="sr-only">{{ playPauseLabel }}</span>
                        </button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="skipBy(10)"
                            :disabled="!canSeek" aria-label="Go forward 10 seconds">
                            <span class="glyphicon glyphicon-forward" aria-hidden="true"></span>
                            <span class="sr-only">Go forward 10 seconds</span>
                        </button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="goToNextSegment"
                            :disabled="!nextSegment" aria-label="Go to next segment">
                            <span class="glyphicon glyphicon-step-forward" aria-hidden="true"></span>
                            <span class="sr-only">Go to next segment</span>
                        </button>
                    </div>
                    <label class="playback-speed" for="audio-audit-speed">
                        <span>Speed</span>
                        <select id="audio-audit-speed" v-model.number="playbackRate" @change="setPlaybackRate">
                            <option :value="0.75">
                                0.75x
                            </option>
                            <option :value="1">
                                1x
                            </option>
                            <option :value="1.25">
                                1.25x
                            </option>
                            <option :value="1.5">
                                1.5x
                            </option>
                            <option :value="2">
                                2x
                            </option>
                        </select>
                    </label>
                    <form class="position-jump" @submit.prevent="playAtPosition">
                        <label>
                            <span>#</span>
                            <input v-model.number="targetSegmentNumber" type="number" min="1" step="1"
                                aria-label="Record number" :aria-describedby="jumpError
                                    ? 'audio-audit-position-error'
                                    : null
                                    " />
                        </label>
                        <label>
                            <span>Offset</span>
                            <input v-model="targetOffset" type="text" inputmode="text" autocomplete="off"
                                placeholder="0:00" aria-label="Playback offset in minutes and seconds"
                                :aria-describedby="jumpError
                                    ? 'audio-audit-position-error'
                                    : null
                                    " />
                        </label>
                        <button type="submit" class="btn btn-outline-secondary btn-sm">
                            Play
                        </button>
                    </form>
                    <p v-if="jumpError" id="audio-audit-position-error" class="position-error" role="alert">
                        {{ jumpError }}
                    </p>
                </div>
            </div>

            <div class="audio-audit-playlist">
                <div v-if="allUnavailable" class="alert alert-info">
                    {{ $t('ReviewInterview.AudioAudit_NoRecordingsAvailable') }}
                </div>

                <ul class="list-unstyled" aria-label="Audio audit recording list">
                    <li v-for="(segment, index) in segments" :key="segment.segmentId">
                        <div v-if="getGapText(index)" class="segment-gap">
                            {{ getGapText(index) }}
                        </div>
                        <button type="button" class="playlist-item" :class="{
                            active:
                                currentSegment &&
                                currentSegment.segmentId ===
                                segment.segmentId,
                            unavailable: segment.unavailable,
                        }" :disabled="segment.unavailable" :aria-current="currentSegment &&
                                currentSegment.segmentId === segment.segmentId
                                " @click="selectSegment(segment)">
                            <span class="segment-info">
                                <span class="segment-start">
                                    <span class="segment-label"># {{ segment.sequenceNumber }}</span>
                                    <span v-if="segment.deviceLocalStartTime" class="segment-time text-muted">
                                        {{
                                            formatDeviceTime(
                                                segment.deviceLocalStartTime,
                                            )
                                        }}
                                    </span>
                                </span>
                                <span class="segment-duration text-muted">
                                    <template v-if="segment.unavailable">
                                        {{
                                            $t(
                                                'ReviewInterview.AudioAudit_SegmentUnavailable',
                                            )
                                        }}
                                    </template>
                                    <template v-else-if="segment.durationLoading">
                                        {{
                                            $t(
                                                'ReviewInterview.AudioAudit_DurationLoading',
                                            )
                                        }}
                                    </template>
                                    <template v-else-if="
                                        segment.duration !== undefined
                                    ">
                                        {{
                                            formatCompactDuration(
                                                segment.duration,
                                            )
                                        }}
                                    </template>
                                    <template v-else>
                                        {{
                                            $t(
                                                'ReviewInterview.AudioAudit_DurationUnavailable',
                                            )
                                        }}
                                    </template>
                                </span>
                            </span>
                        </button>
                    </li>
                </ul>
            </div>
        </template>
    </aside>
</template>

<script>
import { loadAudioMetadata } from './audioAuditMetadata'

export default {
    name: 'AudioAuditPanel',
    emits: ['close'],

    props: {
        interviewId: {
            type: String,
            required: true,
        },
    },

    data() {
        return {
            segments: [],
            currentSegment: null,
            hasAudioAudit: false,
            isPlaying: false,
            currentTime: 0,
            currentDuration: null,
            playbackError: null,
            playbackAnnouncement: '',
            loadState: 'loading',
            isBuffering: false,
            isCurrentSegmentReady: false,
            pendingSeekTime: null,
            pendingSeekValidation: false,
            pendingAutoplay: false,
            isSeeking: false,
            seekPreviewTime: null,
            targetSegmentNumber: null,
            targetOffset: '0:00',
            jumpError: null,
            playbackRate: 1,
            previouslyFocusedElement: null,
        }
    },

    computed: {
        currentSegmentUrl() {
            if (!this.currentSegment) return null
            return this.$hq.AudioAudit.getSegmentUrl(
                this.interviewId,
                this.currentSegment.segmentId
            )
        },

        allUnavailable() {
            return (
                this.segments.length > 0 &&
                this.segments.every((segment) => segment.unavailable)
            )
        },

        previousSegment() {
            return this.findAdjacentSegment(-1)
        },

        nextSegment() {
            return this.findAdjacentSegment(1)
        },

        canSeek() {
            return (
                this.isCurrentSegmentReady &&
                Number.isFinite(this.currentDuration)
            )
        },

        playPauseLabel() {
            return this.isPlaying ? 'Pause recording' : 'Play recording'
        },

        seekAriaValue() {
            const duration =
                this.currentDuration === null
                    ? 'unknown duration'
                    : this.formatTime(this.currentDuration)
            return `${this.formatTime(this.seekBarTime)} of ${duration}`
        },

        seekBarTime() {
            return this.isSeeking ? this.seekPreviewTime : this.currentTime
        },
    },

    async mounted() {
        this.previouslyFocusedElement = document.activeElement
        this.$nextTick(() => this.$refs.closeButton?.focus())
        await this.loadSegments()
    },

    beforeUnmount() {
        this.pauseAudio()
        this.previouslyFocusedElement?.focus?.()
    },

    deactivated() {
        this.pauseAudio()
        this.previouslyFocusedElement?.focus?.()
    },

    methods: {
        async loadSegments() {
            this.loadState = 'loading'
            this.playbackError = null
            this.jumpError = null
            this.currentSegment = null
            this.segments = []

            try {
                const data = await this.$hq.AudioAudit.getInfo(this.interviewId)
                this.hasAudioAudit = data.hasAudioAudit
                if (!data.hasAudioAudit) {
                    this.loadState = 'empty'
                    return
                }

                this.segments = (data.segments || []).map((segment) => ({
                    ...segment,
                    duration: undefined,
                    durationLoading: true,
                    unavailable: false,
                }))

                if (this.segments.length === 0) {
                    this.loadState = 'empty'
                    return
                }

                this.loadState = 'ready'
                this.selectSegment(this.segments[0])
                this.segments.forEach((segment) =>
                    this.preloadDuration(segment)
                )
            } catch {
                this.hasAudioAudit = false
                this.loadState = 'error'
            }
        },

        preloadDuration(segment) {
            const url = this.$hq.AudioAudit.getSegmentUrl(
                this.interviewId,
                segment.segmentId
            )
            loadAudioMetadata(url).then((metadata) => {
                segment.durationLoading = false
                segment.duration = Number.isFinite(metadata.duration)
                    ? metadata.duration
                    : undefined
                segment.unavailable = this.isUnsupportedAudio(
                    metadata.errorCode
                )
            })
        },

        selectSegment(
            segment,
            { autoplay = false, offset = 0, validateOffset = false } = {}
        ) {
            if (segment.unavailable) return

            const shouldPlay = autoplay || this.isPlaying
            this.pauseAudio()
            this.currentSegment = segment
            this.pendingSeekTime = Math.max(0, Number(offset) || 0)
            this.pendingSeekValidation = validateOffset
            this.pendingAutoplay = shouldPlay
            this.currentTime = this.pendingSeekTime
            this.isSeeking = false
            this.seekPreviewTime = null
            this.currentDuration = null
            this.playbackError = null
            this.jumpError = null
            this.isBuffering = shouldPlay
            this.isCurrentSegmentReady = false
            this.targetSegmentNumber = segment.sequenceNumber
            this.playbackAnnouncement = `Record #${segment.sequenceNumber} selected.`

            this.$nextTick(() => {
                if (!this.$refs.audioPlayer) return

                this.$refs.audioPlayer.load()
            })
        },

        findAdjacentSegment(direction) {
            const currentIndex = this.segments.findIndex(
                (segment) =>
                    segment.segmentId === this.currentSegment?.segmentId
            )

            for (
                let index = currentIndex + direction;
                index >= 0 && index < this.segments.length;
                index += direction
            ) {
                if (!this.segments[index].unavailable)
                    return this.segments[index]
            }

            return null
        },

        goToPreviousSegment() {
            if (this.previousSegment) this.selectSegment(this.previousSegment)
        },

        goToNextSegment() {
            if (this.nextSegment) this.selectSegment(this.nextSegment)
        },

        playAtPosition() {
            const segment = this.segments.find(
                (item) =>
                    String(item.sequenceNumber) ===
                    String(this.targetSegmentNumber)
            )
            const offset = this.parseOffset(this.targetOffset)

            if (!segment || segment.unavailable || offset === null) {
                this.jumpError =
                    'Enter an available record number and a valid offset.'
                return
            }

            if (
                Number.isFinite(segment.duration) &&
                offset >= segment.duration
            ) {
                this.jumpError = `Record #${segment.sequenceNumber
                    } is ${this.formatCompactDuration(segment.duration)} long.`
                return
            }

            this.selectSegment(segment, {
                autoplay: true,
                offset,
                validateOffset: true,
            })
        },

        togglePlayPause() {
            if (!this.$refs.audioPlayer) return

            if (this.isPlaying) {
                this.pauseAudio()
                return
            }

            this.requestPlayback()
        },

        requestPlayback() {
            if (!this.$refs.audioPlayer) return

            this.playbackError = null
            if (!this.isCurrentSegmentReady) {
                this.pendingAutoplay = true
                this.isBuffering = true
                this.playbackAnnouncement = 'Loading recording.'
                return
            }

            this.playAudio()
        },

        playAudio() {
            this.$refs.audioPlayer?.play().catch(() => {
                this.isPlaying = false
                this.isBuffering = false
                this.playbackError = this.$t(
                    'ReviewInterview.AudioAudit_PlaybackFailed'
                )
                this.playbackAnnouncement = this.playbackError
            })
        },

        pauseAudio() {
            if (this.$refs.audioPlayer) {
                this.$refs.audioPlayer.pause()
            }
            this.isPlaying = false
        },

        beginSeeking() {
            if (!this.canSeek) return

            this.isSeeking = true
            this.seekPreviewTime = this.currentTime
        },

        previewSeek(value) {
            if (!this.canSeek) return

            this.isSeeking = true
            this.seekPreviewTime = this.getBoundedSeekTime(value)
        },

        commitSeek(value) {
            if (!this.canSeek || !this.$refs.audioPlayer) return

            const seekTime = this.getBoundedSeekTime(value)
            this.isSeeking = false
            this.seekPreviewTime = null
            this.$refs.audioPlayer.currentTime = seekTime
            this.currentTime = seekTime
        },

        getBoundedSeekTime(value) {
            return Math.max(
                0,
                Math.min(this.currentDuration, Number(value) || 0)
            )
        },

        skipBy(seconds) {
            if (
                !this.$refs.audioPlayer ||
                !Number.isFinite(this.currentDuration)
            )
                return

            const seekTime = Math.max(
                0,
                Math.min(this.currentDuration, this.currentTime + seconds)
            )
            this.$refs.audioPlayer.currentTime = seekTime
            this.currentTime = seekTime
        },

        setPlaybackRate() {
            if (this.$refs.audioPlayer) {
                this.$refs.audioPlayer.playbackRate = this.playbackRate
            }
            this.playbackAnnouncement = `Playback speed ${this.playbackRate}x.`
        },

        retryCurrentSegment() {
            if (!this.currentSegment || this.currentSegment.unavailable) return

            this.selectSegment(this.currentSegment, {
                autoplay: true,
                offset: this.currentTime,
            })
        },

        closePanel() {
            this.pauseAudio()
            this.$emit('close')
        },

        onMetadataLoaded() {
            const audio = this.$refs.audioPlayer
            if (audio && Number.isFinite(audio.duration)) {
                this.currentDuration = audio.duration
                if (this.currentSegment) {
                    this.currentSegment.duration = audio.duration
                    this.currentSegment.durationLoading = false
                }

                if (Number.isFinite(this.pendingSeekTime)) {
                    if (
                        this.pendingSeekValidation &&
                        this.pendingSeekTime >= audio.duration
                    ) {
                        audio.currentTime = 0
                        this.currentTime = 0
                        this.pendingAutoplay = false
                        this.jumpError = `Record #${this.currentSegment.sequenceNumber
                            } is ${this.formatCompactDuration(
                                audio.duration
                            )} long.`
                        this.playbackAnnouncement = this.jumpError
                    } else {
                        audio.currentTime = this.pendingSeekTime
                        this.currentTime = audio.currentTime
                    }

                    this.pendingSeekValidation = false
                    this.pendingSeekTime = null
                }
            }
        },

        onCanPlay() {
            this.$refs.audioPlayer.playbackRate = this.playbackRate
            this.isCurrentSegmentReady = true
            this.isBuffering = false

            if (this.pendingAutoplay) {
                this.pendingAutoplay = false
                this.playAudio()
            }
        },

        onPlay() {
            this.isPlaying = true
            this.isBuffering = false
            this.playbackAnnouncement = `Playing record #${this.currentSegment?.sequenceNumber}.`
        },

        onPause() {
            this.isPlaying = false
            if (!this.$refs.audioPlayer?.ended) {
                this.playbackAnnouncement = `Paused record #${this.currentSegment?.sequenceNumber}.`
            }
        },

        onWaiting() {
            if (!this.$refs.audioPlayer?.paused) {
                this.isBuffering = true
                this.playbackAnnouncement = 'Loading audio.'
            }
        },

        onPlaying() {
            this.isPlaying = true
            this.isBuffering = false
        },

        onPlaybackError() {
            this.isPlaying = false
            this.isBuffering = false
            this.isCurrentSegmentReady = false
            this.pendingAutoplay = false
            this.playbackError =
                this.$refs.audioPlayer?.error?.code ===
                    MediaError.MEDIA_ERR_SRC_NOT_SUPPORTED
                    ? this.$t('ReviewInterview.AudioAudit_UnsupportedFormat')
                    : this.$t('ReviewInterview.AudioAudit_PlaybackFailed')
            this.playbackAnnouncement = this.playbackError

            if (
                this.currentSegment &&
                this.isUnsupportedAudio(this.$refs.audioPlayer?.error?.code)
            ) {
                this.currentSegment.unavailable = true
            }
        },

        onTimeUpdate() {
            if (!this.$refs.audioPlayer) return

            if (!this.isSeeking) {
                this.currentTime = this.$refs.audioPlayer.currentTime
            }
        },

        onEnded() {
            if (this.nextSegment) {
                this.selectSegment(this.nextSegment, { autoplay: true })
                return
            }

            this.isPlaying = false
            this.isBuffering = false
            this.playbackAnnouncement = `Record #${this.currentSegment?.sequenceNumber} finished.`
        },

        isUnsupportedAudio(errorCode) {
            return errorCode === 4
        },

        formatTime(seconds) {
            if (!Number.isFinite(seconds)) return '--:--'

            const hours = Math.floor(seconds / 3600)
            const minutes = Math.floor(seconds / 60)
            const remainingSeconds = Math.floor(seconds % 60)
            if (hours > 0) {
                return `${hours}:${Math.floor((seconds % 3600) / 60)
                    .toString()
                    .padStart(2, '0')}:${remainingSeconds
                        .toString()
                        .padStart(2, '0')}`
            }

            return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`
        },

        formatSegmentDuration(seconds) {
            const roundedSeconds = Math.round(seconds)
            const hours = Math.floor(roundedSeconds / 3600)
            const minutes = Math.floor((roundedSeconds % 3600) / 60)
            const remainingSeconds = roundedSeconds % 60
            const parts = []

            if (hours > 0)
                parts.push(`${hours} ${hours === 1 ? 'hour' : 'hours'}`)
            if (minutes > 0)
                parts.push(`${minutes} ${minutes === 1 ? 'minute' : 'minutes'}`)
            if (remainingSeconds > 0 || parts.length === 0) {
                parts.push(
                    `${remainingSeconds} ${remainingSeconds === 1 ? 'second' : 'seconds'
                    }`
                )
            }

            return parts.join(' ')
        },

        formatCompactDuration(seconds) {
            return this.formatTime(Math.round(seconds))
        },

        getGapText(index) {
            if (index === 0) return null

            const previousSegment = this.segments[index - 1]
            const currentSegment = this.segments[index]
            const previousStart = this.parseDeviceTimestamp(
                previousSegment.deviceLocalStartTime
            )
            const currentStart = this.parseDeviceTimestamp(
                currentSegment.deviceLocalStartTime
            )

            if (
                !Number.isFinite(previousSegment.duration) ||
                previousStart === null ||
                currentStart === null
            ) {
                return null
            }

            const gapSeconds = Math.round(
                (currentStart - previousStart) / 1000 -
                previousSegment.duration
            )
            return gapSeconds > 0
                ? `${this.formatSegmentDuration(gapSeconds)} later`
                : null
        },

        parseOffset(value) {
            const parts = String(value).trim().split(':')
            if (
                !parts.length ||
                parts.length > 3 ||
                parts.some((part) => !/^\d+$/.test(part))
            )
                return null

            const values = parts.map(Number)
            if (values.length > 1 && values[values.length - 1] >= 60)
                return null
            if (values.length === 3 && values[1] >= 60) return null

            if (values.length === 3)
                return values[0] * 3600 + values[1] * 60 + values[2]
            if (values.length === 2) return values[0] * 60 + values[1]
            return values[0]
        },

        parseDeviceTimestamp(timestamp) {
            if (!timestamp) return null

            const match = String(timestamp).match(
                /^(\d{4})(\d{2})(\d{2})\D?(\d{2})(\d{2})(\d{2})/
            )
            if (match) {
                const [, year, month, day, hour, minute, second] = match
                return Date.UTC(
                    year,
                    Number(month) - 1,
                    day,
                    hour,
                    minute,
                    second
                )
            }

            const parsed = Date.parse(timestamp)
            return Number.isNaN(parsed) ? null : parsed
        },

        formatDeviceTime(timestamp) {
            const match = String(timestamp || '').match(
                /^(\d{4})(\d{2})(\d{2})\D?(\d{2})(\d{2})(\d{2})/
            )
            if (!match) return timestamp

            const [, year, month, day, hour, minute, second] = match
            return `${year}-${month}-${day} ${hour}:${minute}:${second}`
        },
    },
}
</script>

<style scoped>
.audio-audit-panel {
    position: fixed;
    top: 70px;
    right: 0;
    bottom: 0;
    z-index: 1050;
    width: 340px;
    box-sizing: border-box;
    padding: 15px;
    border-left: 1px solid #d9d9d9;
    background: #fff;
    overflow-y: auto;
}

.audio-audit-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 15px;
}

.audio-audit-header h4 {
    font-family: 'RobotoRegular';
    margin: 0;
}

.audio-audit-state {
    padding: 10px 0;
}

.close-panel {
    width: 40px;
    height: 40px;
    padding: 0;
    font-size: 28px;
    line-height: 1;
}

.audio-audit-player audio {
    width: 100%;
}

.player-controls {
    margin-top: 10px;
}

.now-playing {
    margin: 0 0 8px;
    font-weight: bold;
}

.player-status {
    display: flex;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 10px;
}

.player-buffering {
    margin: 0 0 8px;
    color: #777;
    font-size: 12px;
}

.player-buffering.is-hidden {
    visibility: hidden;
}

.control-buttons {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 10px;
}

.playback-speed {
    display: flex;
    align-items: center;
    gap: 8px;
    margin: 12px 0 0;
    font-size: 12px;
    font-weight: normal;
}

.playback-speed select {
    height: 30px;
    padding: 4px 6px;
    border: 1px solid #ccc;
}

.position-jump {
    display: grid;
    grid-template-columns: minmax(0, 0.7fr) minmax(0, 1fr) auto;
    gap: 8px;
    align-items: end;
    margin-top: 12px;
}

.position-jump label {
    display: block;
    margin: 0;
    font-size: 12px;
    font-weight: normal;
}

.position-jump input {
    display: block;
    width: 100%;
    min-width: 0;
    height: 30px;
    padding: 4px 6px;
    border: 1px solid #ccc;
}

.position-error {
    margin: 8px 0 0;
    color: #a94442;
    font-size: 12px;
}

.audio-audit-playlist {
    margin-top: 15px;
}

.audio-audit-playlist h5 {
    margin: 0 0 8px;
}

.playlist-item {
    display: block;
    width: 100%;
    margin-bottom: 4px;
    padding: 7px 10px;
    border: 1px solid #ededed;
    background: #fff;
    text-align: left;
    cursor: pointer;
}

.playlist-item.active {
    border-color: #337ab7;
    background: #e8f4ff;
    box-shadow: inset 4px 0 0 #337ab7;
}

.playlist-item.active .segment-label {
    font-weight: bold;
}

.playlist-item.unavailable {
    cursor: default;
    opacity: 0.7;
}

.segment-gap {
    margin: 4px 0 8px;
    color: #777;
    font-size: 12px;
    text-align: center;
}

.segment-info {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: center;
    column-gap: 8px;
}

.segment-start {
    display: flex;
    align-items: center;
    min-width: 0;
    gap: 8px;
}

.segment-label {
    white-space: nowrap;
}

.segment-time {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.segment-duration {
    white-space: nowrap;
}

@media (max-width: 767px) {
    .audio-audit-panel {
        top: 0;
        left: 0;
        width: 100%;
        border-left: 0;
    }

    .player-status {
        flex-wrap: wrap;
    }
}
</style>
