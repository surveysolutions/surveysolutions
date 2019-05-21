<template>
  <HqLayout :fixedWidth="true">
    <div slot="headers">
      <h1>
        {{$t('DataExport.DataExport_Title')}}
      </h1>
      <div>
        <p>Data export set will be generated based on selected version of questionaire, timeline and statuses and can contain additional data (DDI structure, paradata, collected binary data)</p>
      </div>
    </div>
     <div class="row" v-if="exportServiceIsUnavailable">
        <div class="col-md-12 mb-30">
          Export service is not available
        </div>
    </div>
    <div class="row" v-else-if="!exportFormIsVisible">
        <div class="col-md-12 mb-30">
          <button  type="button" class="btn btn-success" @click="exportFormIsVisible = true">Generate new export set</button>
        </div>
    </div>
    <form v-if="exportFormIsVisible && !exportServiceIsUnavailable">
      <div class="row">
        <div class="col-md-12">
          <h3>Filters</h3>
          <div class="structure-block">
            <h5>Survey template (questionnaire version)</h5>
            <Typeahead
              control-id="questionnaireId"
              fuzzy
              :placeholder="$t('Common.AllQuestionnaires')"
              data-vv-name="questionnaireId"
              data-vv-as="questionnaire"
              v-validate="'required'" 
              :value="questionnaireId"
              v-on:selected="questionnaireSelected"
              :fetch-url="questionnaireFetchUrl"
            />
            <Typeahead
              control-id="questionnaireVersion"
              ref="questionnaireVersionControl"
              data-vv-name="questionnaireVersion"
              data-vv-as="questionnaireVersion"
              v-validate="'required'"
              :placeholder="$t('Common.AllVersions')"
              :value="questionnaireVersion"
              :fetch-url="questionnaireVersionFetchUrl"
              v-on:selected="questionnaireVersionSelected"
              :disabled="questionnaireVersionFetchUrl == null"
              :selectFirst=true
            />
          </div>
          <div class="structure-block checkbox-block">
            <h5>Status of exported interviews</h5>
            <Typeahead
              control-id="status"
              fuzzy
              data-vv-name="status"
              data-vv-as="status"
              :placeholder="$t('Common.AllStatuses')"
              :value="status"
              :values="statuses"
              v-on:selected="statusSelected"
            />
          </div>
        </div>
      </div>
      <div class="row">
        <div class="col-md-10">
          <h3>Data type</h3>
          <div class="structure-block">
            <div class="data-type-row" v-if="hasInterviews">
              <input class="radio-row" type="radio"  name="dataType" id="surveyData" v-model="dataType" value="surveyData">
              <label for="surveyData" class="" >
                <span class="tick"></span>
                <span class="format-data zip">{{$t('DataExport.MainSurveyDataTitle')}}. {{$t('DataExport.ZipArchiveDescription')}}</span>
              </label>
            </div>
            <div class="data-type-row" v-if="hasInterviews && hasBinaryData">
              <input class="radio-row" type="radio" name="dataType" id="binaryData"  v-model="dataType" value="binaryData">
              <label for="binaryData">
                <span class="tick"></span>
                <span class="format-data zip">Binary Data. Archive with binary data (e.g., pictures, audio)</span>
              </label>
            </div>
            <div class="data-type-row" v-if="questionnaireVersion">
              <input class="radio-row" type="radio" name="dataType" id="ddiData" v-model="dataType" value="ddiData">
              <label for="ddiData">
                <span class="tick"></span>
                <span class="format-data format-data">DDI. Data Documentation Initiative XML data</span>
              </label>
            </div>
            <div class="data-type-row" v-if="hasInterviews">
              <input class="radio-row" type="radio" name="dataType" id="paraData" v-model="dataType" value="paraData">
              <label for="paraData">
                <span class="tick"></span>
                <span class="format-data separated-data">Paradata. Metadata on the interview process (events and timing)</span>
              </label>
            </div>
          </div>
        </div>
      </div>
      <div class="row" v-if="dataType == 'surveyData'">
        <div class="col-md-10">
          <h3>Data format</h3>
          <div class="structure-block">
            <div class="data-type-row">
              <input class="radio-row" type="radio" name="dataFormat" id="separated" v-model="dataFormat" value="Tabular">
              <label for="separated">
                <span class="tick"></span>
                <span class="format-data separated-data">Tab separated data</span>
              </label>
            </div>
            <div class="data-type-row">
              <input class="radio-row" type="radio" name="dataFormat" id="Stata"  v-model="dataFormat" value="Stata">
              <label for="Stata" class="">
                <span class="tick"></span>
                <span class="format-data stata">Stata 10 format (no Unicode)</span>
              </label>
            </div>
            <div class="data-type-row">
              <input class="radio-row" type="radio" name="dataFormat" id="Spss" v-model="dataFormat" value="Spss">
              <label for="Spss">
                <span class="tick"></span>
                <span class="format-data spss">SPSS format</span>
              </label>
            </div>
          </div>
        </div>
      </div>
      <div class="row" v-if="dataType == 'surveyData' || dataType == 'binaryData'">
        <div class="col-md-10">
          <h3>Export file destination</h3>
          <div class="structure-block">
            <div class="export-row">
              <input class="radio-row" type="radio" name="exportDestination" id="download"  v-model="dataDestination" value="zip">
              <label for="download">
                <span class="tick"></span>
                <span class="format-data download-icon">Download</span>
              </label>
            </div>
            <div class="export-row">
              <input class="radio-row" type="radio" name="exportDestination" id="onedrive"  v-model="dataDestination" value="oneDrive">
              <label for="onedrive">
                <span class="tick"></span>
                <span class="format-data onedrive-icon">Upload to OneDrive</span>
              </label>
            </div>
            <div class="export-row">
              <input class="radio-row" type="radio" name="exportDestination" id="dropbox"  v-model="dataDestination" value="dropbox">
              <label for="dropbox">
                <span class="tick"></span>
                <span class="format-data dropbox-icon">Upload to Dropbox</span>
              </label>
            </div>
            <div class="export-row">
              <input class="radio-row" type="radio" name="exportDestination" id="googleDrive" v-model="dataDestination" value="googleDrive">
              <label for="googleDrive">
                <span class="tick"></span>
                <span class="format-data google-icon">Upload to Google Drive</span>
              </label>
            </div>
          </div>
        </div>
      </div>
      <div class="row">
        <div class="col-md-12">
          <div class="structure-block">
              <button type="button" class="btn btn-success" @click="queueExport">add to queue</button>
              <button type="button" class="btn btn-lg btn-link" @click="exportFormIsVisible = false">Cancel</button>
          </div>
        </div>
      </div>
    </form>
    <div class="row" v-if="!exportServiceIsUnavailable">
        <div class="col-md-10">
            <div class="export-sets">
                <div class="no-sets hidden">No generated sets yet</div>
                <h3>Previously generated export sets</h3>
                <p>Every set is a zip archive with all collected interview data and DDI XML structure you an download previously generated reports</p>
                <ExportProcessCard  v-for="result in exportResults" v-bind:key="result.id" :data="result"></ExportProcessCard>
            </div>
        </div>
    </div>
    <div class="row" v-if="!exportServiceIsUnavailable">
        <div class="col-md-8">
            <h3>Data export API</h3>
            <p>You can setup automatic export of collected interview data using our API toolset for more information 
                and inctruction use <a href="#" target="_blank" class="underlined-link">Data Export API reference</a>
            </p>
        </div>
    </div>
  </HqLayout>
</template>

<script>
import Vue from "vue"
import ExportProcessCard from "./ExportProcessCard"
import {mixin as VueTimers} from 'vue-timers'

export default {
    mixins: [VueTimers],
    data() {
        return {
            exportServiceIsUnavailable: true,
            exportFormIsVisible: false,
            dataType: null,
            dataFormat: "Tabular",
            dataDestination: "zip",
            questionnaireId: null,
            questionnaireVersion: null,
            status: null,
            statuses: this.$config.model.statuses,
            exportResults: [],
            isUpdatingDataAvailability: false,
            hasInterviews: false,
            hasBinaryData: false
        };
    },
    timers: {
      updateExportCards: { time: 5000, autostart: true, repeat: true }
    },
    created() {
        var self = this;
        self.$store.dispatch("showProgress");

        this.$http.get(this.$config.model.api.statusUrl)
            .then(function (response) {

              if (response.data)
                self.exportServiceIsUnavailable = false;

              let exportIds = response.data || [];
              for(let i = 0;i< exportIds.length;i++)
              {
                self.exportResults.push({ 
                  id: exportIds[i]
                });
              }
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
    },
    computed: {
        questionnaireFetchUrl() {
            return this.$config.model.api.questionnairesUrl;
        },
        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.$config.model.api.questionnairesUrl}/${this.questionnaireId.key}`;
            return null;
        }
    },
    watch: {},
    methods: {
      updateExportCards(){
        var self = this;
        this.$http.get(this.$config.model.api.statusUrl)
            .then((response) => {

              self.exportServiceIsUnavailable = response.data == null;

              let exportIds = response.data || [];
              if (exportIds.length > 0)
              {
                var existingJobIndex = 0;
                var incomingJobIndex = 0;
                while(incomingJobIndex < exportIds.length && existingJobIndex < self.exportResults.length)
                {
                  var existingProcessId = self.exportResults[existingJobIndex].id;
                  var newProcessId = exportIds[incomingJobIndex];
                  if (existingProcessId == newProcessId)
                  {
                    existingJobIndex++;
                    incomingJobIndex++;
                  }
                  else if (existingProcessId < newProcessId)
                  {
                    self.exportResults.splice(existingJobIndex, 0, { id: newProcessId})
                    existingJobIndex++;
                    incomingJobIndex++;
                  }
                  else{
                    existingJobIndex++;
                  }
                }
                for(let i=incomingJobIndex;i<exportIds.length;i++)
                {
                  self.exportResults.push({ id: exportIds[i]});
                }
              }
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
      },
      async queueExport(){
        var self = this;
        var validationResult = await this.$validator.validateAll();
        if (validationResult)
        {
            const exportParams = self.getExportParams(
              self.questionnaireId.key,
              self.questionnaireVersion.key,
              self.dataType,
              self.dataFormat,
              self.dataDestination,
              (self.status || { value: null}).value
            );

            self.$store.dispatch("showProgress");

            this.$http.post(this.$config.model.api.updateSurveyDataUrl, null, { params: exportParams })
                .then(function (response) {
                    self.$validator.reset();
                    self.exportFormIsVisible = false;
                    const jobId = (response.data || { JobId: 0 }).JobId;
                    self.exportResults.splice(0, 0,{ 
                      id: jobId
                    });
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self);
                })
                .then(function () {
                    self.$store.dispatch("hideProgress");
                });
        }else{
            var fieldName = this.errors.items[0].field;
            const $firstFieldWithError = $("#"+fieldName);
            $firstFieldWithError.focus();
        }        
      },
      getExportParams(questionnaireId, questionnaireVersion, dataType, dataFormat, dataDestination, status){
        const dataFormatNum = {  
          Tabular: 1,
          Stata: 2,
          Spss: 3,
          Binary: 4,
          Ddi: 5,
          Paradata: 6
        }
        var result = dataFormatNum.Tabular;
        switch(dataType)
        {
          case "surveyData": result = dataFormatNum[dataFormat]; break;
          case "binaryData": result = dataFormatNum.Binary; break;
          case "ddiData": result = dataFormatNum.Ddi; break;
          case "paraData":  result = dataFormatNum.Paradata; break;
        }

        return {
              id: questionnaireId,
              version: questionnaireVersion,
              format: dataFormat, 
              status: status
            };
      },
        statusSelected(newValue) {
            this.status = newValue;
        },
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue;
            if (this.questionnaireVersion)
              this.updateDataAvalability();
        },
        updateDataAvalability(){
          this.isUpdatingDataAvailability = true;
          
          this.$http.get(this.$config.model.api.dataAvailabilityUrl, { params: {
            id: this.questionnaireId.key,
            version: this.questionnaireVersion.key
          } })
            .then((response) => {
              this.hasInterviews = response.data.hasInterviews;
              this.hasBinaryData = response.data.hasBinaryData;
              this.dataType = this.hasInterviews ? "surveyData" : "ddi";
            })
            .catch((error) => {
                Vue.config.errorHandler(error, self);
            })
            .then(() => {
                this.isUpdatingDataAvailability = false;
            });
        }
    },
    components: {ExportProcessCard}
};
</script>
