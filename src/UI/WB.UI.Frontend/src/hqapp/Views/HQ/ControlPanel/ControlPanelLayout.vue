<template>
    <HqLayout :hasFilter="true">
        <Filters slot="filters"
            :title="''">
            <ol class="list-unstyled">
                <li>
                    <router-link to="/ControlPanel/Configuration">{{$t('Pages.Configuration')}}</router-link>
                </li>
                <li>
                    <router-link to="/ControlPanel/TabletInfos">{{$t('Pages.PackagesInfo_Header')}}</router-link>
                </li>
                <li>
                    <router-link to="/ControlPanel/AppUpdates">{{$t('Pages.AppUpdates')}}</router-link>
                </li>
                <li>
                    <router-link to="/ControlPanel/CreateAdmin">{{$t('Pages.CreateAdmin')}}</router-link>
                </li>
            </ol>
        </Filters>
        <router-view />
    </HqLayout>
</template>
