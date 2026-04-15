<template>
    <div v-if="isReconnecting" class="reconnecting-banner">
        <span class="reconnecting-banner__spinner" aria-hidden="true"></span>
        <span class="reconnecting-banner__text">{{ $t('WebInterviewUI.Reconnecting') }}</span>
        <span v-if="attemptCount > 0" class="reconnecting-banner__attempt">
            {{ $t('WebInterviewUI.ReconnectAttempt', { count: attemptCount }) }}
        </span>
        <button type="button" class="reconnecting-banner__reload" @click="reload">
            {{ $t('WebInterviewUI.Reload') }}
        </button>
    </div>
</template>

<script lang="js">
export default {
    name: 'ReconnectingBanner',

    computed: {
        isReconnecting() {
            return this.$store.state.webinterview.connection.isReconnecting
        },
        attemptCount() {
            return this.$store.state.webinterview.connection.reconnectAttemptCount
        },
    },

    methods: {
        reload() {
            location.reload()
        },
    },
}
</script>

<style scoped>
.reconnecting-banner {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 3000;
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 20px;
    background-color: #e8a000;
    color: #1a1a1a;
    font-size: 14px;
}

.reconnecting-banner__spinner {
    display: inline-block;
    width: 16px;
    height: 16px;
    border: 2px solid rgba(0, 0, 0, 0.25);
    border-top-color: #1a1a1a;
    border-radius: 50%;
    animation: reconnecting-spin 0.8s linear infinite;
    flex-shrink: 0;
}

@keyframes reconnecting-spin {
    to {
        transform: rotate(360deg);
    }
}

.reconnecting-banner__attempt {
    opacity: 0.85;
}

.reconnecting-banner__reload {
    margin-left: auto;
    background: rgba(0, 0, 0, 0.1);
    border: 1px solid rgba(0, 0, 0, 0.35);
    color: #1a1a1a;
    padding: 4px 14px;
    border-radius: 3px;
    cursor: pointer;
    font-size: 13px;
}

.reconnecting-banner__reload:hover {
    background: rgba(0, 0, 0, 0.2);
}
</style>
