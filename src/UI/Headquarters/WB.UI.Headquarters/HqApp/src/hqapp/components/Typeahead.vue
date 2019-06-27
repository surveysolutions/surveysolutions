<template>
    <div class="combo-box" :title="value == null ? '' : value.value" :id="controlId">
        <div class="btn-group btn-input clearfix">
            <button type="button" :id="buttonId"
                    class="btn dropdown-toggle"
                    data-toggle="dropdown"
                    :disabled="disabled">
                <span data-bind="label"
                      v-if="value == null"
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
                    :placeholder="$t('Common.Search')"
                    @input="updateOptionsList" 
                    @keyup.down="onSearchBoxDownKey" 
                    v-model="searchTerm" />
                </li>
                <li v-if="forceLoadingState">
                    <a>{{ $t("Common.Loading") }}</a>
                </li>
                <template v-if="!forceLoadingState" >
                    <li v-for="option in options" :key="keyFunc(option.item)">
                        <a 
                        :class="[option.item.iconClass]"
                        href="javascript:void(0);"
                            @click="selectOption(option.item)" 
                        v-html="highlight(option, searchTerm)"
                        @keydown.up="onOptionUpKey"></a>
                    </li>
                </template>
                <li v-if="isLoading">
                    <a>{{ $t("Common.Loading") }}</a>
                </li>
                <li v-if="!isLoading && options.length === 0">
                    <a>{{ $t("Common.NoResultsFound") }}</a>
                </li>
            </ul>
        </div>
        <button v-if="value != null && !noClear"
                class="btn btn-link btn-clear"
                type="button"
                @click="clear">
            <span></span>
        </button>
    </div>
</template>

<script>
import Fuse from "fuse.js";

export default {
    name: "Typeahead",

    props: {
        fetchUrl: String,
        controlId: {
            type: String,
            required: true
        },
        value: Object,
        placeholder: String,
        ajaxParams: Object,
        forceLoadingState: {
            type: Boolean,
            default: false
        },
        values: Array,
        noSearch: Boolean,
        noClear: Boolean,
        disabled: Boolean,
        fuzzy: {
            type: Boolean,
            default: false
        },
        selectFirst: {
            type: Boolean,
            default: false
        },
        selectedKey: {
            type: String,
            default: null
        }
    },
    watch: {
        fetchUrl (val) {
            this.clear();
            if(val) {
                this.fetchOptions();
            }
            else {
                this.options.splice(0, this.options.length);
            }
        }
    },
    data() {
        return {
            options: [],
            isLoading: false,
            searchTerm: ""
        };
    },

    computed: {
        inputId() {
            return `sb_${this.controlId}`;
        },
        buttonId() {
            return `btn_trigger_${this.controlId}`;
        },
        placeholderText() {
            return this.placeholder || "Select";
        }
    },

    mounted() {
        const jqEl = $(this.$el);
        const focusTo = jqEl.find(`#${this.inputId}`);

        jqEl.on("shown.bs.dropdown", () => {
            focusTo.focus();
            this.fetchOptions(this.searchTerm);
        });

        jqEl.on("hidden.bs.dropdown", () => {
            this.searchTerm = "";
        });

        this.fuseOptions = {
            shouldSort: true,
            includeMatches: true,
            threshold: this.fuzzy ? 0.35 : 0,
            location: 0,
            distance: 100,
            maxPatternLength: 32,
            minMatchCharLength: 1,
            keys: ["value"]
        };

        if(this.selectedKey != null) {
            this.fetchOptions(this.searchTerm, this.selectedKey);
        }
    },

    methods: {
        onSearchBoxDownKey() {
            const $firstOptionAnchor = $(this.$refs.dropdownMenu)
                .find("a")
                .first();
            $firstOptionAnchor.focus();
        },
        onOptionUpKey(event) {
            const isFirstOption =
                $(event.target)
                    .parent()
                    .index() === 1;

            if (isFirstOption) {
                this.$refs.searchBox.focus();
                event.stopPropagation();
            }
        },

        fetchOptions(filter = "", selectedKey = null) {
            if (this.values) {
                if (filter != "") {
                    const fuse = new Fuse(this.values, this.fuseOptions);
                    this.options = this.setOptions(fuse.search(filter), false);
                } else {
                    this.options = this.setOptions(this.values);
                }

                if(selectedKey != null) {
                    this.selectByKey(selectedKey);
                }

                return;
            }

            this.isLoading = true;
            const requestParams = _.assign(
                { query: filter, cache: false},
                this.ajaxParams
            );

            return this.$http
                .get(this.fetchUrl, { params: requestParams })
                .then(response => {
                    if(response != null && response.data != null) {
                        this.options = this.setOptions(response.data.options || []);
                        if (this.selectFirst && this.options.length > 0)
                        {
                            this.selectOption(this.options[0].item);
                        }
                    }

                    this.isLoading = false;
                    
                    if(selectedKey != null) {
                        this.selectByKey(selectedKey);
                    }
                })
                .catch(() => (this.isLoading = false));
        },

        setOptions(values, wrap = true) {
            if (wrap == false) return values;

            return _.chain(values).filter(v => v != null).map(v => {
                return {
                    item: v,
                    matches: null
                };
            }).value();
        },

        clear() {
            this.$emit("selected", null, this.controlId);
            this.searchTerm = "";
        },
        selectOption(value) {
            this.$emit("selected", value, this.controlId);
        },
        selectByKey(key) {
            const itemToSelect = _.find(this.options, o => o.item.key == key)
            if(itemToSelect != null) {
                this.selectOption(itemToSelect.item)
            }
        },
        updateOptionsList(e) {
            this.fetchOptions(e.target.value);
        },
        highlight(option, searchTerm) {
            if (option.matches == null) {
                const encodedTitle = _.escape(option.item.value);

                if (searchTerm) {
                    const safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));
                    const iQuery = new RegExp(safeSearchTerm, "ig");

                    return encodedTitle.replace(iQuery, matchedTxt => {
                        return `<strong>${matchedTxt}</strong>`;
                    });
                }

                return encodedTitle;
            } else {
                return generateHighlightedText(
                    option.item.value,
                    option.matches[0].indices
                );
            }
        },
        keyFunc(item) {
            return item == null ? 'null' : item.key + "$" + item.value 
        }
    }
};

// Does not account for overlapping highlighted regions, if that exists at all O_o..
function generateHighlightedText(
    text,
    regions,
    start = "<strong>",
    end = "</strong>"
) {
    if (!regions) return text;

    var content = "",
        nextUnhighlightedRegionStartingIndex = 0;

    regions.forEach(region => {
        content +=
            "" +
            text.substring(nextUnhighlightedRegionStartingIndex, region[0]) +
            start +
            text.substring(region[0], region[1] + 1) +
            end +
            "";

        nextUnhighlightedRegionStartingIndex = region[1] + 1;
    });

    content += text.substring(nextUnhighlightedRegionStartingIndex);

    return content;
}
</script>
