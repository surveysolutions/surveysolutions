<template>
    <div>
        <h1>Hello</h1>
        <ul>
            <li>
                <router-link to='/prefilled/580eba69f1844f1892147342f568bb0f$1'>Preffiled</router-link>
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
