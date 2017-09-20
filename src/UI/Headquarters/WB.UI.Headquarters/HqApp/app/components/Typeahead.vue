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
                      :class="[value.iconClass]"
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
                           @keyup.down="onSearchBoxDownKey"
                           v-model="searchTerm" />
                </li>
                <li v-for="option in options"
                    :key="option.key">
                    <a 
                       :class="[option.iconClass]"
                       href="javascript:void(0);"
                       @click="selectOption(option)"
                       v-html="highlight(option.value, searchTerm)"
                       @keydown.up="onOptionUpKey"></a>
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
export default {
    name: 'Typeahead',

    props: {
        fetchUrl: String,
        controlId: String,
        value: Object,
        placeholder: String,
        ajaxParams: Object,
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
            const $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first();
            $firstOptionAnchor.focus();
        },
        onOptionUpKey(event) {
            const isFirstOption = $(event.target).parent().index() === 1;

            if (isFirstOption) {
                this.$refs.searchBox.focus();
                event.stopPropagation();
            }
        },
        fetchOptions(filter = "") {
            if (this.values) {
                this.options = this.values;
                return;
            }

            this.isLoading = true;
            const requestParams = Object.assign({ query: filter, cache: false }, this.ajaxParams);

            this.$http.get(this.fetchUrl, {params: requestParams})
                .then(response => {
                    this.options = response.data.options || [];
                    this.isLoading = false;
                })
                .catch(() => this.isLoading = false)
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
}

</script>