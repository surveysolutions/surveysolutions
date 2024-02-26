<template>
    <div class="row">
        <div class="col-md-6">
            <div class="checkbox-in-column">
                <input id="cb-is-timestamp" type="checkbox" class="wb-checkbox" v-model="activeQuestion.isTimestamp" />
                <label for="cb-is-timestamp"><span></span>{{ $t('QuestionnaireEditor.QuestionCurrentTime') }}</label>
                <help link="isTimestamp"></help>
            </div>
        </div>
        <div class="col-md-6 ng-scope inline-inputs" v-if="!activeQuestion.isTimestamp">
            <div class="form-group checkbox checkbox-in-column">
                <label for="dt-default-date">{{ $t('QuestionnaireEditor.QuestionDefaultDate') }}</label>
                <!--input id="dt-default-date" type="text" jqdatepicker v-model="activeQuestion.defaultDate"
                    class="form-control small-date-input" /-->

                <DatePicker v-model="activeQuestion.defaultDate" :popover="{ visibility: 'click' }" :hide-time-header="true"
                    timezone="UTC">
                    <template #default="{ inputValue, inputEvents, togglePopover }">
                        <div class="date-wrapper">
                            <input id="dt-default-date" type="search" :value="formatedDate" v-on="inputEvents"
                                readonly="readonly" class="form-control small-date-input" />
                            <button type="button" class="btn cross date-cross" v-if="activeQuestion.defaultDate != null"
                                @click="activeQuestion.defaultDate = null;"></button>
                        </div>
                    </template>
                </DatePicker>
                <help link="defaultDate"></help>
            </div>
        </div>
    </div>
</template>

<style lang="scss">
.vc-pane-container {

    button.vc-title,
    button.vc-arrow {
        background-color: inherit;
    }
}

.vc-nav-popover-container {
    button {
        background-color: inherit;
    }
}

.row .form-group .date-wrapper {
    position: relative;
    display: inline-block;

    button.date-cross {
        position: absolute;
        right: 2px;
        display: inline;
        top: 9px;
        width: 17px;
        height: 17px;
    }
}
</style>

<script>

import { DatePicker } from 'v-calendar';
import 'v-calendar/style.css';

import moment from 'moment';

import Help from './../Help.vue'

export default {
    name: 'DateTimeQuestion',
    components: {
        Help,
        DatePicker,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
    computed: {
        formatedDate() {
            const date = this.activeQuestion.defaultDate;
            if (date)
                return moment.utc(date).format('YYYY-MM-DD');
            return null;
        }
    }
}
</script>
