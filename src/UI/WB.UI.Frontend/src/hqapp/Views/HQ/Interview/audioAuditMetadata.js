const metadataRequests = new Map()

export function loadAudioMetadata(url) {
    const existingRequest = metadataRequests.get(url)
    if (existingRequest) return existingRequest

    const request = new Promise(resolve => {
        const audio = new Audio()

        const cleanup = () => {
            audio.removeEventListener('loadedmetadata', onMetadataLoaded)
            audio.removeEventListener('error', onError)
            audio.removeAttribute('src')
            audio.load()
        }

        const finish = result => {
            cleanup()
            resolve(result)
        }

        const onMetadataLoaded = () => {
            finish({
                duration: Number.isFinite(audio.duration) ? audio.duration : null,
                errorCode: null,
            })
        }

        const onError = () => {
            finish({
                duration: null,
                errorCode: audio.error?.code ?? null,
            })
        }

        audio.preload = 'metadata'
        audio.addEventListener('loadedmetadata', onMetadataLoaded)
        audio.addEventListener('error', onError)
        audio.src = url
    })

    metadataRequests.set(url, request)
    request.then(result => {
        if (!Number.isFinite(result.duration)) {
            metadataRequests.delete(url)
        }
    })

    return request
}