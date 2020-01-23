<template>
    <HqLayout :hasFilter="true" :hasRow="false">
        <aside slot="filters" class="filters">
            <div class="foldback-button" id="hide-filters">
                <span class="arrow"></span>
                <span class="arrow"></span>
                <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
            </div>
            <div class="filters-container">
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
                </ol>
            </div>
        </aside>
        <router-view />
    </HqLayout>
</template>
