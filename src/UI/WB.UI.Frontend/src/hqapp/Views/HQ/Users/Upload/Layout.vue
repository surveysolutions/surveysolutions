<template>
    <main>
        <div class="container">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a>{{ $t('MainMenu.Users') }}</a>
                        </li>
                    </ol>
                    <h1>{{ $t('UploadUsers.Title') }}</h1>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-7">
                    <slot name="title" />
                </div>
            </div>
            <router-view />
        </div>
    </main>
</template>


<script>
import { nextTick } from 'vue'
export default {
    mounted() {
        nextTick(() => {
            window.ajustNoticeHeight()
            window.ajustDetailsPanelHeight()
        })
    },
}
</script>
