<template>
    <div class="breadcrumbs-container">
        <div class="breadcrumbs">
            <ul>
                <li v-for="(breadcrumb, index) in breadcrumbs" :key="breadcrumb.id">

                    <router-link v-if="index == 0" :to="{
                        name: 'group',
                        params: {
                            entityId: breadcrumb.id
                        }
                    }">{{ breadcrumb.title }}</router-link>

                    <router-link v-if="index != 0 && !breadcrumb.isRoster" :to="{
                        name: 'group',
                        params: {
                            entityId: breadcrumb.id
                        }
                    }">{{ breadcrumb.title }}</router-link>

                    <router-link v-if="index != 0 && breadcrumb.isRoster" :to="{
                        name: 'roster',
                        params: {
                            entityId: breadcrumb.id
                        }
                    }">{{ breadcrumb.title }}</router-link>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
export default {
    name: 'Breadcrumbs',
    props: {
        breadcrumbs: { type: Array, required: true }
    }
};
</script>
