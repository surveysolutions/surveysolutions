<template>
    <aside v-if="hasAudioAudit" class="audio-audit-panel">
        <div class="audio-audit-header">
            <h4>{{ $t('ReviewInterview.AudioAudit_ViewRecordings') }}</h4>
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
                <div class="time-display">
                    {{ formatTime(currentTime) }} / {{ currentDuration !== null ? formatTime(currentDuration) : '--:--'
                    }}
                </div>
                <input type="range" class="seek-bar form-range" :max="currentDuration || 0" :value="currentTime"
                    @input="seekTo($event.target.value)" />
                <div class="control-buttons">
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
                </div>
            </div>
        </div>

        <div class="audio-audit-playlist">
            <div v-if="allUnavailable" class="alert alert-info">
                {{ $t('ReviewInterview.AudioAudit_NoRecordingsAvailable') }}
            </div>

            <ul class="list-unstyled">
                <li v-for="segment in segments" :key="segment.segmentId" class="playlist-item" :class="{
                    active: currentSegment && currentSegment.segmentId === segment.segmentId,
                    unavailable: segment.unavailable,
                }" @click="!segment.unavailable && selectSegment(segment)">
                    <div class="segment-info">
                        <span class="segment-label">
                            {{ $t('ReviewInterview.AudioAudit_SegmentLabel', { number: segment.sequenceNumber }) }}
                        </span>
                        <span v-if="segment.deviceLocalStartTime" class="segment-time text-muted">
                            {{ $t('ReviewInterview.AudioAudit_DeviceTime', {
                                time:
                                    formatDeviceTime(segment.deviceLocalStartTime) }) }}
                        </span>
                        <span class="segment-duration text-muted">
                            <template v-if="segment.unavailable">
                                {{ $t('ReviewInterview.AudioAudit_SegmentUnavailable') }}
                            </template>
                            <template v-else-if="segment.durationLoading">
                                {{ $t('ReviewInterview.AudioAudit_DurationLoading') }}
                            </template>
                            <template v-else-if="segment.duration !== undefined">
                                {{ formatTime(segment.duration) }}
                            </template>
                            <template v-else>
                                {{ $t('ReviewInterview.AudioAudit_DurationUnavailable') }}
                            </template>
                        </span>
                    </div>
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

        selectSegment(segment) {
            if (segment.unavailable) return

            const wasPlaying = this.isPlaying
            this.pauseAudio()
            this.currentSegment = segment
            this.currentTime = 0
            this.currentDuration = null
            this.playbackError = null

            this.$nextTick(() => {
                if (!this.$refs.audioPlayer) return

                this.$refs.audioPlayer.load()
                if (wasPlaying) {
                    this.$refs.audioPlayer.play().catch(() => { })
                }
            })
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
            this.isPlaying = false
        },

        formatTime(seconds) {
            if (!Number.isFinite(seconds)) return '--:--'

            const minutes = Math.floor(seconds / 60)
            const remainingSeconds = Math.floor(seconds % 60)
            return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`
        },

        formatDeviceTime(timestamp) {
            if (!timestamp || timestamp.length < 15) return timestamp

            const date = timestamp.substring(0, 8)
            const time = timestamp.substring(9, 15)
            return `${date.substring(0, 4)}-${date.substring(4, 6)}-${date.substring(6, 8)} ${time.substring(0, 2)}:${time.substring(2, 4)}:${time.substring(4, 6)}`
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
    z-index: 6;
    width: 340px;
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

.audio-audit-player audio {
    width: 100%;
}

.player-controls {
    margin-top: 10px;
}

.time-display {
    margin-bottom: 10px;
}

.control-buttons {
    display: flex;
    gap: 10px;
    margin-top: 10px;
}

.audio-audit-playlist {
    margin-top: 15px;
}

.playlist-item {
    padding: 10px 0;
    border-bottom: 1px solid #ededed;
    cursor: pointer;
}

.playlist-item.active .segment-label {
    font-weight: bold;
}

.playlist-item.unavailable {
    cursor: default;
    opacity: 0.7;
}

.segment-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
}
</style>
