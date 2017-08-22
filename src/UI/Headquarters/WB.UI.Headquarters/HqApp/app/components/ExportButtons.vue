<template>
    <div class="dropdown">
        <button class="btn btn-default dropdown-toggle" type="button" id="dropdownMenu1" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
            {{$t('Pages.ExportButton')}}
            <span class="caret"></span>
        </button>
        <ul class="dropdown-menu" aria-labelledby="dropdownMenu1">
            <li>
                <a v-bind:href="$store.state.exportUrls.excel">{{$t('Pages.ExportToExcel')}}</a>
            </li>
            <li>
                <a v-bind:href="$store.state.exportUrls.csv">{{$t('Pages.ExportToCsv')}}</a>
            </li>
            <li>
                <a v-bind:href="$store.state.exportUrls.tab">{{$t('Pages.ExportToTab')}}</a>
            </li>
        </ul>
    </div>
</template>

<script>
export default {
    props: {
       table: {
            type: Object,
            reqiured: true
        },
    }
}
</script>

