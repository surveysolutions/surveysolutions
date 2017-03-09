<template>
    <div class="combo-box">
        <div class="btn-group btn-input clearfix">
            <button type="button" class="btn dropdown-toggle" data-toggle="dropdown">
                <span data-bind="label" v-if="value === null" class="gray-text">Click to answer</span>
                <span data-bind="label" v-else>{{value}}</span>
            </button>
            <ul ref="dropdownMenu" class="dropdown-menu" role="menu">
                <li>
                    <input type="text" ref="searchBox" :id="inputId" placeholder="Search" input="updateOptionsList" v-on:keyup.down="onSearchBoxDownKey" v-model="searchTerm" />
                </li>
                <li v-for="option in options" :key="option.userId">
                    <a href="javascript:void(0);" v-on:click="selectOption(option.userId)" v-html="highlight(option.userName, searchTerm)" v-on:keydown.up="onOptionUpKey"></a>
                </li>
                <li v-if="isLoading">
                    <a>Loading...</a>
                </li>
                <li v-if="!isLoading && options.length === 0">
                    <a>No results found</a>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
    Vue = require('vue');

    module.exports = {
        name: 'user-selector',
        props: ['fetchUrl', 'controlId', 'value'],
        data: function () {
            return {
                options: [],
                isLoading: false,
                searchTerm: ''
            };
        },
        computed: {
            inputId: function () {
                return `sb_${this.controlId}`;
            },
            valueTitle: function () {

            }
        },
        mounted: function () {
            this.fetchUsers();
        },
        methods: {
            onSearchBoxDownKey(event) {
                var $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first();
                $firstOptionAnchor.focus();
            },
            onOptionUpKey(event) {
                var isFirstOption = $(event.target).parent().index() === 1;

                if (isFirstOption) {
                    this.$refs.searchBox.focus();
                    event.stopPropagation();
                }
            },
            fetchUsers: function (filter) {
                this.isLoading = true;
                this.$http.get(this.fetchUrl)
                    .then(response => {
                        this.options = response.body.users || [];
                        this.isLoading = false;
                    }, response => {
                        console.log("error");
                        this.isLoading = false;
                    });

            },
            clear: function () {

            },
            selectOption: function (value) {
                this.$emit('selected', value, this.controlId);
            },
            updateOptionsList(e) {
                this.fetchUsers(e.target.value);
            },
            highlight: function (title, searchTerm) {
                var encodedTitle = _.escape(title);
                if (searchTerm) {
                    var safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));

                    var iQuery = new RegExp(safeSearchTerm, "ig");
                    return encodedTitle.replace(iQuery, (matchedTxt, a, b) => {
                        return `<strong>${matchedTxt}</strong>`;
                    });
                }

                return encodedTitle;
            }
        }
    };
</script>