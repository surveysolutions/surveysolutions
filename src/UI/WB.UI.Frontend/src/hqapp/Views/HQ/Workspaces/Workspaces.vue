<template>
    <HqLayout :hasFilter="false"
        :title="$t('Dashboard.Workspaces')">

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            noSelect
            :noPaging="false">
        </DataTables>

    </HqLayout>
</template>

<script>
export default {

}
</script>
