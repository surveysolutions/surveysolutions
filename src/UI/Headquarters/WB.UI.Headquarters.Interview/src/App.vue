<template>
    <div>
        <h1>Hello</h1>
        <ul>
            <li>
                <router-link to='/prefilled/1234'>Preffiled</router-link>
            </li>
            </li>
            <li>
                <router-link to='/start/start'>Start</router-link>
            </li>

        </ul>
        <router-view></router-view>
    </div>
</template>
<script lang='ts'>
    export default {
        name: 'app'
        // $hq-style-path: "../../WB.UI.Headquarters/Dependencies/css/_mixins.scss"
    }
</script>
<style lang='scss'>
    @import "style.scss"
</style>
