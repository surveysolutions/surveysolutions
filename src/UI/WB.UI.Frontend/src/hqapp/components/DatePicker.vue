<template>
    <div class="form-date input-group"
        id="dates-range">
        <input
            type="text"
            :id="id"
            :disabled="disabled"
            class="form-control flatpickr-input"
            readonly="readonly"
            :name="name"
            :placeholder="placeholder"
            :required="required"
            v-model="mutableValue"
            data-input/>
        <button type="submit"
            class="btn btn-link btn-clear">
            <span></span>
        </button>
        <span class="input-group-addon"
            data-toggle>
            <span class="calendar"></span>
        </span>
    </div>
</template>

<script type="text/javascript">
import Flatpickr from 'flatpickr'
import {browserLanguage} from '~/shared/helpers'
import FlatpickrLocale from 'flatpickr/dist/l10n'
import {assign} from 'lodash'

Flatpickr.localize(FlatpickrLocale[browserLanguage])
// You have to import css yourself

export default {
    props: {
        value: {
            default: null,
            required: true,
            validate(value) {
                return (
                    value === null ||
                    value instanceof Date ||
                    typeof value === 'string' ||
                    value instanceof String ||
                    value instanceof Array
                )
            },
        },
        // https://chmln.github.io/flatpickr/options/
        config: {
            type: Object,
            default: () => ({
                wrap: false,
            }),
        },
        disabled: Boolean,
        placeholder: {
            type: String,
            default: '',
        },
        inputClass: {
            type: [String, Object],
            default: '',
        },
        name: {
            type: String,
            default: 'date-time',
        },
        required: {
            type: Boolean,
            default: false,
        },
        withClear: {
            type: Boolean,
            default: false,
        },
        id: {
            type: String,
        },
        clearLabel: {
            type: String,
            default: null,
        },
    },
    data() {
        return {
            mutableValue: this.value,
            fp: null,
        }
    },
    mounted() {
        // Load flatPickr if not loaded yet

        if (!this.fp) {
            // Bind on parent element if wrap is true
            const elem = this.config.wrap ? this.$el.parentNode : this.$el
            const self = this
            let config = this.config
            if (this.withClear) {
                config = assign(
                    {
                        onReady: function(dateObj, dateStr, instance) {
                            $('.flatpickr-calendar').each(function() {
                                var $this = $(this)
                                if ($this.find('.flatpickr-clear').length < 1) {
                                    $this.append(
                                        '<div class="flatpickr-clear" style=" cursor: pointer;">' +
                                            (self.clearLabel || 'Clear') +
                                            '</div>'
                                    )
                                    $this.find('.flatpickr-clear').on('click', function() {
                                        instance.close()
                                        self.$emit('clear')
                                        self.fp.setDate(null, true)
                                    })
                                }
                            })
                        },
                    },
                    this.config
                )
            }

            this.fp = new Flatpickr(elem, config)
        }
    },
    beforeDestroy() {
        // Free up memory
        if (this.fp) {
            this.fp.destroy()
            this.fp = null
        }
    },
    watch: {
        /**
         * Watch for any config changes and redraw date-picker
         *
         * @param newConfig Object
         */
        config(newConfig) {
            this.fp.config = assign(this.fp.config, newConfig)
            this.fp.redraw()
            this.fp.setDate(this.value, true)
        },
        /**
         * Watch for value changed by date-picker itself and notify parent component
         *
         * @param newValue
         */
        mutableValue(newValue) {
            this.$emit('input', newValue)
        },
        /**
         * Watch for changes from parent component and update DOM
         *
         * @param newValue
         */
        value(newValue) {
            this.fp && this.fp.setDate(newValue, true)
        },
    },
}
</script>

