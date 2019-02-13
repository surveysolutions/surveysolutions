<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.EmailProvidersTitle')">
        
        <div class="row contain-input">
            
            <div class="col-sm-7 ">
            <form v-if="!loading">
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" name="provider" class="wb-radio" type="radio" v-model="provider" id="provider_none"  value="none">
                        <label for="provider_none"><span class="tick"></span>{{ $t('Settings.EmailProvider_None') }}</label>
                    </div>
                </div>
                <p>You cannot send invitations and notifications</p>
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio" name="provider" type="radio" v-model="provider" id="provider_amazon"  value="amazon">
                        <label for="provider_amazon"><span class="tick"></span>{{ $t('Settings.EmailProvider_Amazon') }}</label>
                    </div>
                </div>
                <div v-if="provider === 'amazon'">
                    <div class="form-group">
                        <label>Sender email address </label>
                        <input v-validate="'required|email'" name="senderAddress" class="form-control" id="senderAddress" type="text" v-model="senderAddress" maxlength="200" />
                        <span>{{ errors.first('senderAddress') }}</span>
                    </div>
                    <h4>AWS access keys</h4>
                    <div class="form-group">
                        <label>Access key ID</label>
                        <input v-validate="'required'" class="form-control" name="awsAccessKeyId" id="awsAccessKeyId" type="text" v-model="awsAccessKeyId" maxlength="200" />
                        <span>{{ errors.first('awsAccessKeyId') }}</span>
                    </div>
                    <div class="form-group">
                        <label>Secret access key</label>
                        <input v-validate="'required'" name="awsSecretAccessKey" class="form-control" id="awsSecretAccessKey" type="text" v-model="awsSecretAccessKey" maxlength="200" />
                        <span>{{ errors.first('awsSecretAccessKey') }}</span>
                    </div>
                </div>
               <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio"  name="provider" type="radio" v-model="provider" id="provider_sendgrid"  value="sendgrid">
                        <label for="provider_sendgrid"><span class="tick"></span>{{ $t('Settings.EmailProvider_Sendgrid') }}</label>
                    </div>
                </div>
                <div  v-if="provider === 'sendgrid'">
                    <div class="form-group">
                        <label>Sender email address </label>
                        <input v-validate="'required|email'" name="senderAddress" class="form-control" id="senderAddress" type="text" v-model="senderAddress" maxlength="200" />
                        <span>{{ errors.first('senderAddress') }}</span>
                    </div>
                    <div class="form-group">
                        <label>API key</label>
                        <input v-validate="'required'" name="sendGridApiKey" class="form-control" id="sendGridApiKey" type="text" v-model="sendGridApiKey" maxlength="200" />
                        <span>{{ errors.first('sendGridApiKey') }}</span>
                    </div>
                </div>
                <button class="btn btn-success" type="button" :disabled="!isFormDirty"   @click="save">Save</button>
                </form>
            </div>
        </div>
    </HqLayout>
</template>

<script>
export default {
  data() {
    return {
        loading: true,
        provider: 'none',
        senderAddress: null,
        awsAccessKeyId: null,
        awsSecretAccessKey: null,
        sendGridApiKey: null
    };
  },
  async created() {
    const response = await this.$http.get(this.$config.model.api.getSettings);
    const settings = response.data || {};
    this.loading = false;
    this.provider = (settings.provider|| "").toLocaleLowerCase();
    this.senderAddress = settings.senderAddress;
    this.awsAccessKeyId = settings.awsAccessKeyId;
    this.awsSecretAccessKey = settings.awsSecretAccessKey;
    this.sendGridApiKey = settings.sendGridApiKey;
  },
  computed: {
    isFormDirty() {
      return Object.keys(this.fields).some(key => this.fields[key].dirty);
    }
  },
  methods: {
    async save(){
        var validationResult = await this.$validator.validate();
        if (validationResult)
        {
            const settings = {
                provider: this.provider,
                senderAddress: this.senderAddress,
                awsAccessKeyId: this.awsAccessKeyId,
                awsSecretAccessKey: this.awsSecretAccessKey,
                sendGridApiKey: this.sendGridApiKey
            };
            await this.$http.post(this.$config.model.api.updateSettings, settings);
        }else{
            var fieldName = this.errors.items[0].field;
            const $firstFieldWithError = $("#"+fieldName);
            $firstFieldWithError.focus();
        }        
    }
  }
};
</script>