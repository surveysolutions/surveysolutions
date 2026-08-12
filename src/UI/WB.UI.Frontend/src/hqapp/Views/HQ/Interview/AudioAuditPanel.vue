<template>
    <aside v-if="hasAudioAudit" class="audio-audit-panel">
        <div class="audio-audit-header">
            <h4>Audio audit records</h4>
            <button type="button" class="btn btn-link close-panel" @click="$emit('close')"
                :aria-label="$t('ReviewInterview.AudioAudit_CloseRecordings')">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>

        <div class="audio-audit-player" v-if="currentSegment">
            <audio ref="audioPlayer" :src="currentSegmentUrl" controlsList="nodownload"
                @loadedmetadata="onMetadataLoaded" @error="onPlaybackError" @timeupdate="onTimeUpdate" @ended="onEnded"
                preload="metadata" style="display:none">
            </audio>

            <div v-if="playbackError" class="alert alert-warning">
                {{ playbackError }}
            </div>

            <div class="player-controls">
                <div class="player-status">
                    <span>Position: {{ formatTime(currentTime) }}</span>
                    <span>Duration: {{ currentDuration !== null ? formatTime(currentDuration) : '--:--' }}</span>
                </div>
                <input type="range" class="seek-bar form-range" :max="currentDuration || 0" :value="currentTime"
                    @input="seekTo($event.target.value)" :disabled="currentDuration === null" />
                <div class="control-buttons">
                    <button type="button" class="btn btn-outline-secondary btn-sm" @click="goToPreviousSegment"
                        :disabled="!previousSegment" aria-label="Go to previous segment">
                        Previous
                    </button>
                    <button type="button" class="btn btn-outline-secondary btn-sm" @click="skipBackward"
                        :aria-label="$t('ReviewInterview.AudioAudit_SkipBackward')">
                        -10s
                    </button>
                    <button type="button" class="btn btn-primary" @click="togglePlayPause">
                        {{ isPlaying ? '⏸' : '▶' }}
                    </button>
                    <button type="button" class="btn btn-outline-secondary btn-sm" @click="skipForward"
                        :aria-label="$t('ReviewInterview.AudioAudit_SkipForward')">
                        +10s
                    </button>
                    <button type="button" class="btn btn-outline-secondary btn-sm" @click="goToNextSegment"
                        :disabled="!nextSegment" aria-label="Go to next segment">
                        Next
                    </button>
                </div>
                <form class="position-jump" @submit.prevent="playAtPosition">
                    <label>
                        <span>#</span>
                        <input v-model.number="targetSegmentNumber" type="number" min="1" step="1"
                            aria-label="Record number" />
                    </label>
                    <label>
                        <span>Offset</span>
                        <input v-model="targetOffset" type="text" inputmode="numeric" placeholder="0:00"
                            aria-label="Playback offset" />
                    </label>
                    <button type="submit" class="btn btn-outline-secondary btn-sm">
                        Play
                    </button>
                </form>
            </div>
        </div>

        <div class="audio-audit-playlist">
            <div v-if="allUnavailable" class="alert alert-info">
                {{ $t('ReviewInterview.AudioAudit_NoRecordingsAvailable') }}
            </div>

            <ul class="list-unstyled">
                <li v-for="(segment, index) in segments" :key="segment.segmentId">
                    <div v-if="getGapText(index)" class="segment-gap">
                        {{ getGapText(index) }}
                    </div>
                    <button type="button" class="playlist-item" :class="{
                        active: currentSegment && currentSegment.segmentId === segment.segmentId,
                        unavailable: segment.unavailable,
                    }" :disabled="segment.unavailable"
                        :aria-current="currentSegment && currentSegment.segmentId === segment.segmentId"
                        @click="selectSegment(segment)">
                        <span class="segment-info">
                            <span class="segment-start">
                                <span class="segment-label"># {{ segment.sequenceNumber }}</span>
                                <span v-if="segment.deviceLocalStartTime" class="segment-time text-muted">
                                    {{ formatDeviceTime(segment.deviceLocalStartTime) }}
                                </span>
                            </span>
                            <span class="segment-duration text-muted">
                                <template v-if="segment.unavailable">
                                    {{ $t('ReviewInterview.AudioAudit_SegmentUnavailable') }}
                                </template>
                                <template v-else-if="segment.durationLoading">
                                    {{ $t('ReviewInterview.AudioAudit_DurationLoading') }}
                                </template>
                                <template v-else-if="segment.duration !== undefined">
                                    {{ formatCompactDuration(segment.duration) }}
                                </template>
                                <template v-else>
                                    {{ $t('ReviewInterview.AudioAudit_DurationUnavailable') }}
                                </template>
                            </span>
                        </span>
                    </button>
                </li>
            </ul>
        </div>
    </aside>
</template>

<script>
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
            pendingSeekTime: null,
            targetSegmentNumber: null,
            targetOffset: '0:00',
        }
    },

    computed: {
        currentSegmentUrl() {
            if (!this.currentSegment) return null
            return this.$hq.AudioAudit.getSegmentUrl(this.interviewId, this.currentSegment.segmentId)
        },

        allUnavailable() {
            return this.segments.length > 0 && this.segments.every(segment => segment.unavailable)
        },

        previousSegment() {
            return this.findAdjacentSegment(-1)
        },

        nextSegment() {
            return this.findAdjacentSegment(1)
        },
    },

    async mounted() {
        await this.loadSegments()
    },

    methods: {
        async loadSegments() {
            try {
                const data = await this.$hq.AudioAudit.getInfo(this.interviewId)
                this.hasAudioAudit = data.hasAudioAudit
                if (!data.hasAudioAudit) return

                this.segments = (data.segments || []).map(segment => ({
                    ...segment,
                    duration: undefined,
                    durationLoading: true,
                    unavailable: false,
                }))

                if (this.segments.length > 0) {
                    this.selectSegment(this.segments[0])
                }

                this.segments.forEach(segment => this.preloadDuration(segment))
            } catch {
                this.hasAudioAudit = false
            }
        },

        preloadDuration(segment) {
            const audio = new Audio()
            audio.preload = 'metadata'
            audio.src = this.$hq.AudioAudit.getSegmentUrl(this.interviewId, segment.segmentId)
            audio.addEventListener('loadedmetadata', () => {
                segment.durationLoading = false
                segment.duration = Number.isFinite(audio.duration) ? audio.duration : undefined
            })
            audio.addEventListener('error', () => {
                segment.durationLoading = false
                segment.unavailable = true
            })
        },

        selectSegment(segment, { autoplay = false, offset = 0 } = {}) {
            if (segment.unavailable) return

            const shouldPlay = autoplay || this.isPlaying
            this.pauseAudio()
            this.currentSegment = segment
            this.pendingSeekTime = Math.max(0, Number(offset) || 0)
            this.currentTime = this.pendingSeekTime
            this.currentDuration = null
            this.playbackError = null
            this.targetSegmentNumber = segment.sequenceNumber

            this.$nextTick(() => {
                if (!this.$refs.audioPlayer) return

                this.$refs.audioPlayer.load()
                if (shouldPlay) {
                    this.$refs.audioPlayer.play().catch(() => { })
                }
            })
        },

        findAdjacentSegment(direction) {
            const currentIndex = this.segments.findIndex(segment =>
                segment.segmentId === this.currentSegment?.segmentId)

            for (let index = currentIndex + direction;
                index >= 0 && index < this.segments.length;
                index += direction) {
                if (!this.segments[index].unavailable) return this.segments[index]
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
            const segment = this.segments.find(item =>
                String(item.sequenceNumber) === String(this.targetSegmentNumber))
            const offset = this.parseOffset(this.targetOffset)

            if (!segment || segment.unavailable || offset === null) {
                this.playbackError = 'Enter an available record number and a valid offset.'
                return
            }

            this.selectSegment(segment, { autoplay: true, offset })
        },

        togglePlayPause() {
            if (!this.$refs.audioPlayer) return

            if (this.isPlaying) {
                this.pauseAudio()
                return
            }

            this.$refs.audioPlayer.play()
                .then(() => {
                    this.isPlaying = true
                })
                .catch(() => {
                    this.playbackError = this.$t('ReviewInterview.AudioAudit_PlaybackFailed')
                })
        },

        pauseAudio() {
            if (this.$refs.audioPlayer) {
                this.$refs.audioPlayer.pause()
            }
            this.isPlaying = false
        },

        skipBackward() {
            if (!this.$refs.audioPlayer) return
            this.$refs.audioPlayer.currentTime = Math.max(0, this.$refs.audioPlayer.currentTime - 10)
        },

        skipForward() {
            if (!this.$refs.audioPlayer) return

            const duration = this.$refs.audioPlayer.duration
            const safeMax = Number.isFinite(duration) ? duration : this.$refs.audioPlayer.currentTime
            this.$refs.audioPlayer.currentTime = Math.min(safeMax, this.$refs.audioPlayer.currentTime + 10)
        },

        seekTo(value) {
            if (!this.$refs.audioPlayer) return
            this.$refs.audioPlayer.currentTime = Number(value)
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
                    audio.currentTime = Math.min(audio.duration, this.pendingSeekTime)
                    this.currentTime = audio.currentTime
                    this.pendingSeekTime = null
                }
            }
        },

        onPlaybackError() {
            this.isPlaying = false
            this.playbackError =
                this.$refs.audioPlayer?.error?.code === MediaError.MEDIA_ERR_SRC_NOT_SUPPORTED
                    ? this.$t('ReviewInterview.AudioAudit_UnsupportedFormat')
                    : this.$t('ReviewInterview.AudioAudit_PlaybackFailed')

            if (this.currentSegment) {
                this.currentSegment.unavailable = true
            }
        },

        onTimeUpdate() {
            if (!this.$refs.audioPlayer) return

            this.currentTime = this.$refs.audioPlayer.currentTime
            this.isPlaying = !this.$refs.audioPlayer.paused
        },

        onEnded() {
            if (this.nextSegment) {
                this.selectSegment(this.nextSegment, { autoplay: true })
                return
            }

            this.isPlaying = false
        },

        formatTime(seconds) {
            if (!Number.isFinite(seconds)) return '--:--'

            const hours = Math.floor(seconds / 3600)
            const minutes = Math.floor(seconds / 60)
            const remainingSeconds = Math.floor(seconds % 60)
            if (hours > 0) {
                return `${hours}:${Math.floor((seconds % 3600) / 60).toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
            }

            return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`
        },

        formatSegmentDuration(seconds) {
            const roundedSeconds = Math.round(seconds)
            const hours = Math.floor(roundedSeconds / 3600)
            const minutes = Math.floor((roundedSeconds % 3600) / 60)
            const remainingSeconds = roundedSeconds % 60
            const parts = []

            if (hours > 0) parts.push(`${hours} ${hours === 1 ? 'hour' : 'hours'}`)
            if (minutes > 0) parts.push(`${minutes} ${minutes === 1 ? 'minute' : 'minutes'}`)
            if (remainingSeconds > 0 || parts.length === 0) {
                parts.push(`${remainingSeconds} ${remainingSeconds === 1 ? 'second' : 'seconds'}`)
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
            const previousStart = this.parseDeviceTimestamp(previousSegment.deviceLocalStartTime)
            const currentStart = this.parseDeviceTimestamp(currentSegment.deviceLocalStartTime)

            if (!Number.isFinite(previousSegment.duration) || previousStart === null || currentStart === null) {
                return null
            }

            const gapSeconds = Math.round((currentStart - previousStart) / 1000 - previousSegment.duration)
            return gapSeconds > 0 ? `${this.formatSegmentDuration(gapSeconds)} later` : null
        },

        parseOffset(value) {
            const parts = String(value).trim().split(':')
            if (!parts.length || parts.length > 3 || parts.some(part => !/^\d+$/.test(part))) return null

            const values = parts.map(Number)
            if (values.length > 1 && values[values.length - 1] >= 60) return null
            if (values.length === 3 && values[1] >= 60) return null

            if (values.length === 3) return values[0] * 3600 + values[1] * 60 + values[2]
            if (values.length === 2) return values[0] * 60 + values[1]
            return values[0]
        },

        parseDeviceTimestamp(timestamp) {
            if (!timestamp) return null

            const match = String(timestamp).match(/^(\d{4})(\d{2})(\d{2})\D?(\d{2})(\d{2})(\d{2})/)
            if (match) {
                const [, year, month, day, hour, minute, second] = match
                return Date.UTC(year, Number(month) - 1, day, hour, minute, second)
            }

            const parsed = Date.parse(timestamp)
            return Number.isNaN(parsed) ? null : parsed
        },

        formatDeviceTime(timestamp) {
            const match = String(timestamp || '').match(/^(\d{4})(\d{2})(\d{2})\D?(\d{2})(\d{2})(\d{2})/)
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
    margin: 0;
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

.player-status {
    display: flex;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 10px;
}

.control-buttons {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 10px;
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
