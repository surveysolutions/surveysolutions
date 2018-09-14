<template>
    <div class="combo-box" :title="value == null ? '' : value.value">
        <div class="btn-group btn-input clearfix">
            <button type="button"
                    class="btn dropdown-toggle"
                    data-toggle="dropdown">
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
                <li v-if="!forceLoadingState" v-for="option in options"
                    :key="option.item.key">
                    <a 
                       :class="[option.item.iconClass]"
                       href="javascript:void(0);"
                        @click="selectOption(option.item)" 
                       v-html="highlight(option, searchTerm)"
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
        <button v-if="value != null && !noClear"
                class="btn btn-link btn-clear"
                @click="clear">
            <span></span>
        </button>
    </div>
</template>

<script>
import Fuse from "fuse.js";

export default {
    name: 'Typeahead',

  props: {
    fetchUrl: String,
    controlId: String,
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
    fuzzy: {
        type: Boolean,
        default: false
    }
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

    this.fuseOptions = {
        shouldSort: true,
        includeMatches: true,
        threshold: this.fuzzy ? 0.35 : 0,
        location: 0, distance: 100,
        maxPatternLength: 32, minMatchCharLength: 1,
        keys: ["value"]        
    };
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
        
        if(filter != ""){
            const fuse = new Fuse(this.values, this.fuseOptions)
            this.options = this.setOptions(fuse.search(filter), false)            
        }
        else {
            this.options = this.setOptions(this.values)
        }
        return;
      }

      this.isLoading = true;
      const requestParams = Object.assign({ query: filter, cache: false }, this.ajaxParams);

      this.$http.get(this.fetchUrl, {params: requestParams})
        .then(response => {
            this.options = this.setOptions(response.data.options || []);
            this.isLoading = false;
      }).catch(() => this.isLoading = false)      
    },

    setOptions(values, wrap = true){
        if(wrap == false) return values
        
        return _.map(values, v => {
            return {
                item: v,
                matches: null
            }
        })
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
    highlight(option, searchTerm) {
        if(option.matches == null){
            const encodedTitle = _.escape(option.item.value);

            if (searchTerm) {
                const safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));
                const iQuery = new RegExp(safeSearchTerm, "ig");

                return encodedTitle.replace(iQuery, (matchedTxt) => {
                    return `<strong>${matchedTxt}</strong>`;
                });
            }

            return encodedTitle;
        } else {          
            return generateHighlightedText(option.item.value, option.matches[0].indices)
        }      
    }    
  }
}

// Does not account for overlapping highlighted regions, if that exists at all O_o..
function generateHighlightedText(text, regions, start = "<strong>", end = "</strong>") {
  if(!regions) return text;

  var content = '', nextUnhighlightedRegionStartingIndex = 0;

  regions.forEach((region) => {
    content += '' + text.substring(nextUnhighlightedRegionStartingIndex, region[0]) +
     start +
        text.substring(region[0], region[1] + 1) +
     end + '';

    nextUnhighlightedRegionStartingIndex = region[1] + 1;
  });

  content += text.substring(nextUnhighlightedRegionStartingIndex);

  return content;
}

</script>
