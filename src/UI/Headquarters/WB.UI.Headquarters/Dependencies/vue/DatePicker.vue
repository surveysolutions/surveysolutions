<template>
    <div class="form-date input-group">
        <input type="text" :class="inputClass" :placeholder="placeholder" :value="value" @input="onInput" />
        <span class="input-group-addon">
            <span class="calendar"></span>
        </span>
    </div>
</template>

<script>
import Flatpickr from 'flatpickr'

export default {
    props: {
        inputClass: {
            type: String
        },
        placeholder: {
            type: String,
            default: ''
        },
        options: {
            type: Object,
            default: () => { return {} }
        },
        value: {
            type: String,
            default: ''
        }
    },
  data () {
      return {
          fp: null
      }
  },
    computed: {
        fpOptions () {
            return JSON.stringify(this.options)
        }
    },
    watch: {
        fpOptions (newOpt) {
            const option = JSON.parse(newOpt)
            for (let o in option) {
                this.fp.set(o, option[o])
            }
        }
    },
  mounted () {
      const self = this
      const origOnValUpdate = this.options.onValueUpdate
      this.fp = new Flatpickr(this.$el.querySelector('input'), Object.assign(this.options, {
          onValueUpdate () {
              self.onInput(self.$el.querySelector('input').value)
              if (typeof origOnValUpdate === 'function') {
                  origOnValUpdate()
              }
          }
      }))
      this.$emit('FlatpickrRef', this.fp)
  },
  destroyed () {
      this.fp.destroy()
      this.fp = null
  },
    methods: {
        onInput (e) {
            const selectedDates = this.fp.selectedDates || [];
            const left = selectedDates.length > 0 ? selectedDates[0] : null;
            const right = selectedDates.length > 1 ? selectedDates[1] : null;
            this.$emit('input', (typeof e === 'string' ? e : e.target.value), left, right)
        }
    }
}
</script>