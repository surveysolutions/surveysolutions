<template>
    <main>
        <div class="container">
            <div class="row">
                <div class="page-header">
                    <ol class="breadcrumb">
                        <li>
                            <a href="#">{{$t('MainMenu.TeamsAndRoles')}}</a>
                        </li>
                    </ol>
                    <h1>{{$t('UploadUsers.Title')}}</h1>
                    <p>
                        <slot name="title" />
                    </p>
                </div>
            </div>
            <router-view />
        </div>
    </main>
</template>


<script>
export default {};
</script>
