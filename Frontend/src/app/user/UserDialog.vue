<template>
  <v-dialog
    ref="userDialog"
    v-model="dialog"
    persistent
    width="600"
    @keydown.esc="$emit('close-dialog')"
  >
    <v-card :loading="loading">
      <v-card-title class="title">
        <span v-if="edit">{{ $t('setting.userDialog.dialogTitleEdit') }}</span>
        <span v-else>{{ $t('setting.userDialog.dialogTitle') }}</span>   
        <v-spacer/>
        <v-btn icon @click="$emit('close-dialog')">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-card-title>
      <v-switch
        v-if="edit"
        v-model="user.Enabled"
        :label="user.Enabled ? $t('setting.userDialog.userEnabled') : $t('setting.userDialog.userDisabled')"
        class="px-2 py-1 search-field-2"
      />
      <v-form ref="refUserForm">
        <v-text-field
          v-model="user.UserName"
          :disabled="edit"
          :label="this.$t('setting.userDialog.usernameLabel')"
          :rules="[
            validators.required, 
            validators.usernameValidator,
            validators.maxMinLengthValidator(user.UserName, 5, 30)
          ]"
          required
          prepend-icon="fa-id-card"
          outlined
          dense
          clearable
          minlenth="5"
          maxlength="30"
          counter="30"
          autofocus
          class="px-2 py-1 search-field-2"
        ></v-text-field>
        <v-text-field
          v-model.trim="user.Name"
          :label="this.$t('setting.userDialog.nameLabel')"
          :rules="[
            validators.required, 
            validators.alphaWhiteSpaceValidator, 
            validators.maxMinLengthValidator(user.Name, 5, 30)
          ]"
          required
          prepend-icon="fa-input-text"
          outlined
          dense
          clearable
          minlenth="5"
          maxlength="30"
          counter="30"
          class="px-2 py-1 search-field-2"
        ></v-text-field>
        <v-text-field
          v-model.trim="user.Description"
          :label="this.$t('setting.userDialog.description')"
          :rules="[
            validators.required, 
            validators.alphaWhiteSpaceValidator, 
            validators.maxMinLengthValidator(user.Description, 5, 30)
          ]"
          required
          prepend-icon="fa-circle-info"
          outlined
          dense
          clearable
          minlenth="5"
          maxlength="30"
          counter="30"
          class="px-2 py-1 search-field-2"
        ></v-text-field>
        <v-select
          v-model="user.Role"
          item-text="text"
          item-value="role"
          required
          :items="roles"
          :label="this.$t('setting.userDialog.roleLabel')"
          prepend-icon="fa-user-shield"
          outlined
          dense
          hide-details
          class="px-2 py-1"
        >
        </v-select>
        <v-divider/>
      </v-form>
      <v-form v-if="!edit" ref="refPasswordForm">
        <v-text-field
          v-model="user.Password"
          :label="$t('setting.userDialog.passwordLabel')"
          prepend-icon="fa-key"
          minlength="8"
          maxlength="20"
          counter="20"
          outlined
          dense
          class="px-2 py-1"
          :rules="[
            validators.maxMinLengthValidator(user.Password, 8, 30),
            validators.passwordValidator
          ]"
          required
          clearable
          :append-icon="showPassword ? 'fa-eye-slash' : 'fa-eye'"
          :type="showPassword ? 'text' : 'password'"
          @click:append="showPassword = !showPassword"
        >
        </v-text-field>
        <v-divider></v-divider>
        <v-text-field
          v-model="user.Confirm"
          :label="this.$t('setting.userDialog.confirmPasswordLabel')"
          :rules="[
            validators.maxMinLengthValidator(user.Confirm, 8, 30),
            validators.passwordValidator,
            validators.confirmedValidator(user.Confirm, user.Password)
          ]"
          required
          prepend-icon="fa-key"
          minlength="8"
          maxlength="20"
          counter="20"
          outlined
          dense
          clearable
          class="px-2 py-1"
          :append-icon="showPasswordConfirm ? 'fa-eye-slash' : 'fa-eye'"
          :type="showPasswordConfirm ? 'text' : 'password'"
          @click:append="showPasswordConfirm = !showPasswordConfirm"
        >
        </v-text-field>
      </v-form>
      <v-divider/>
      <v-card-actions>
        <v-btn
          color="error"
          class="text-lg-h6 pa-2"
          @click="$emit('close-dialog')"
        >
          {{ this.$t('common.cancel') }}
        </v-btn>
        <v-spacer/>

        <v-btn
          v-if="!edit"
          :loading="loading"
          color="primary"
          class="font-weight-bold text-lg-h6 pa-2"
          @click="createUser()"
        >
          {{ this.$t('common.save') }}
        </v-btn>
        <v-btn
          v-else
          :loading="loading"
          color="warning"
          class="font-weight-bold black--text text-lg-h6 pa-2"
          @click="updateUser()"
        >
          {{ this.$t('common.edit') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import {
  required,
  emailValidator,
  passwordValidator,
  confirmedValidator,
  alphaWhiteSpaceValidator,
  lengthValidator,
  maxMinLengthValidator,
  usernameValidator
} from '@/configs/validation'
import { reactive } from '@vue/composition-api'
import { createUser, updateUser } from '@/api/users'
import { mapActions, mapGetters } from 'vuex'

export default {
  props: {
    show: {
      type: Boolean,
      default: false
    },
    edit: {
      type: Boolean,
      default: false
    },
    value: {
      type: Object,
      default: null
    }
  },
  setup() {
    const user = new reactive({
      UserName:'',
      Name: '',
      Description: '',
      Password: '',
      Confirm: '',
      Enabled: true,
      Role: '',
      ID: ''
    })
    return {
      user,
      validators: {
        required,
        passwordValidator,
        confirmedValidator,
        alphaWhiteSpaceValidator,
        emailValidator,
        lengthValidator,
        maxMinLengthValidator,
        usernameValidator
      }
    }
  },
  data () {
    return {
      dialog: false,
      loading: false,
      changePassword: false,
      showPassword: false,
      showPasswordConfirm: false
    }
  },
  computed: {
    roles() {
      return [
        { text: this.$t('roles.Admin'), role: 'Admin' },
        { text: this.$t('roles.User'), role: 'User' },
        { text: this.$t('roles.OrderOffice'), role: 'OrderOffice' }
      ]
    }
  },
  watch: {
    show: {
      handler(val) {
        this.dialog = val
        if (this.edit) {
          this.user.ID = this.value.ID
          this.user.Description = this.value.Description
          this.user.Name = this.value.Name
          this.user.UserName = this.value.UserName
          this.user.Enabled = this.value.Enabled
          this.user.Role = this.value.Role
        } else {
          this.cleanFields()
        }
      }
    }
  },
  mounted() {
    this.dialog = this.show
  },
  methods: {
    ...mapActions('app',['showSuccess','showError']),
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    cleanFields() {
      console.log(this.$refs)
      this.$refs.refUserForm.reset()
      this.$refs.refPasswordForm.reset()

    },
    createUser() {
      const user = {
        Name: this.user.Name,
        UserName: this.user.UserName,
        Description: this.user.Description,
        Role: this.user.Role,
        Enabled: true,
        Password: this.user.Password,
        ConfirmPassword: this.user.Confirm
      }
      this.loading = true
      createUser(user)
        .then(res => {
          this.showSuccess()
          this.$emit('close-dialog', true)
        })
        .catch(err => this.showError(err))
        .finally(() => this.loading = false)
      
    },
    updateUser() {
      const user = {
        Name: this.user.Name,
        Description: this.user.Description,
        Role: this.user.Role,
        Enabled: this.user.Enabled,
        Password: this.user.Password,
        ConfirmPassword: this.user.Confirm
      }
      this.loading = true
      updateUser(this.user.ID, user)
        .then(res => {
          this.showSuccess()
          this.$emit('close-dialog', true)
        })
        .catch(err => this.showError(err))
        .finally(() => this.loading = false)
    },
    verifyPassword() {

    }
  }
}
</script>
