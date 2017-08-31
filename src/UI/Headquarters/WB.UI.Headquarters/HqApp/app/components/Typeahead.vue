<template>
    <div class="combo-box">
        <div class="btn-group btn-input clearfix">
            <button type="button"
                    class="btn dropdown-toggle"
                    data-toggle="dropdown">
                <span data-bind="label"
                      v-if="value === null"
                      class="gray-text">{{placeholderText}}</span>
                <span data-bind="label"
                      v-else>{{value.value}}</span>
            </button>
            <ul ref="dropdownMenu"
                class="dropdown-menu"
                role="menu">
                <li v-if="!noSearch">
                    <input type="text"
                           ref="searchBox"
                           :id="inputId"
                           placeholder="Search"
                           @input="updateOptionsList"
                           v-on:keyup.down="onSearchBoxDownKey"
                           v-model="searchTerm" />
                </li>
                <li v-for="option in options"
                    :key="option.key">
                    <a href="javascript:void(0);"
                       v-on:click="selectOption(option)"
                       v-html="highlight(option.value, searchTerm)"
                       v-on:keydown.up="onOptionUpKey"></a>
                </li>
                <li v-if="isLoading">
                    <a>{{ $t("Common.Loading") }}</a>
                </li>
                <li v-if="!isLoading && options.length === 0">
                    <a>{{ $t("Common.NoResultsFound") }}</a>
                </li>
            </ul>
        </div>
        <button v-if="value !== null"
                class="btn btn-link btn-clear"
                @click="clear">
            <span></span>
        </button>
    </div>
</template>

<script>
module.exports = {
    name: 'user-selector',
    props: {
        fetchUrl: String,
        controlId: String,
        value: Object,
        placeholder: String,
        ajaxParams: String,
        values: Array,
        noSearch: Boolean
    },
    data() {
        return {
            options: [],
            isLoading: false,
            searchTerm: ''
        };
    },
    computed: {
        inputId() {
            return `sb_${this.controlId}`;
        },
        placeholderText() {
            return this.placeholder || "Select";
        }
    },
    mounted() {
        const jqEl = $(this.$el)
        const focusTo = jqEl.find(`#${this.inputId}`)

        jqEl.on('shown.bs.dropdown', () => {
            focusTo.focus()
            this.fetchOptions(this.searchTerm)
        })

        jqEl.on('hidden.bs.dropdown', () => {
            this.searchTerm = ""
        })
    },
    methods: {
        onSearchBoxDownKey() {
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
        fetchOptions(filter = "") {
            if(this.values){
                this.options = this.values;
                return;
            }

            this.isLoading = true;
            var requestParams = Object.assign({ query: filter, cache: false }, this.ajaxParams);
            
            $.get(this.fetchUrl, requestParams)
                .done(response => {
                    this.options = response.body.options || [];
                    this.isLoading = false;
                })
                .always(() => this.isLoading = false)
        },
        clear() {
            this.$emit('selected', null, this.controlId);
            this.searchTerm = "";
        },
        selectOption(value) {
            this.$emit('selected', value, this.controlId);
        },
        updateOptionsList(e) {
            this.fetchOptions(e.target.value);
        },
        highlight(title, searchTerm) {
            var encodedTitle = _.escape(title);
            if (searchTerm) {
                var safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));

                var iQuery = new RegExp(safeSearchTerm, "ig");
                return encodedTitle.replace(iQuery, (matchedTxt) => {
                    return `<strong>${matchedTxt}</strong>`;
                });
            }

            return encodedTitle;
        }
    }
};
</script>