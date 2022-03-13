<template>
  <v-dialog :value="dialog" max-width="600px" @click:outside="closeDialog">
    <v-card
      :class="{ 'grey lighten-2': dragover }"
      @drop.prevent="onDrop($event)"
      @dragover.prevent="dragover = true"
      @dragenter.prevent="dragover = true"
      @dragleave.prevent="dragover = false"
    >
      <v-card-text>
        <v-row class="d-flex flex-column" dense align="center" justify="center">
          <v-icon color="info" :class="[dragover ? 'mt-2, mb-6' : 'mt-5']" size="60">
            fa-solid fa-cloud-arrow-up
          </v-icon>
          <p>
            {{ $t('email.uploadHint') }}
          </p>
        </v-row>
        <v-virtual-scroll
          v-if="uploadedFiles.length > 0"
          :items="$enumerable(uploadedFiles).OrderByDescending(x => x.size).ToArray()"
          height="300"
          item-height="50"
        >
          <template v-slot:default="{ item }">
            <v-list-item :key="item.name">
              <v-list-item-content>
                <v-list-item-title>
                  {{ item.name }}

                </v-list-item-title>
              </v-list-item-content>
              <v-list-item-action-text>
                <span class="ml-1 text-lg-body-1 text--secondary">
                  {{ item.size | formatByte }}
                </span>
              </v-list-item-action-text>
              <v-list-item-action>
                <v-btn icon @click.stop="removeFile(item.name)">
                  <v-icon color="red">fa-solid fa-circle-xmark</v-icon>
                </v-btn>
              </v-list-item-action>
            </v-list-item>

            <v-divider></v-divider>
          </template>
        </v-virtual-scroll>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>

        <v-btn icon @click="closeDialog">
          <v-icon id="close-button">fa-xmark</v-icon>
        </v-btn>

        <v-btn icon @click.stop="submit">
          <v-icon id="upload-button">fa-arrow-up-from-line</v-icon>
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  name: 'Upload',
  props: {
    dialog: {
      type: Boolean,
      required: true
    }
  },
  data() {
    return {
      dragover: false,
      uploadedFiles: []
    }
  },
  methods: {
    closeDialog() {
      // Remove all the uploaded files
      this.uploadedFiles = []
      // Close the dialog box
      this.$emit('update:dialog', false)
    },
    removeFile(fileName) {
      // Find the index of the
      const index = this.uploadedFiles.findIndex(
        (file) => file.name === fileName
      )

      // If file is in uploaded files remove it
      if (index > -1) this.uploadedFiles.splice(index, 1)
    },
    onDrop(e) {
      console.log(e.dataTransfer.files)
      this.dragover = false
      // If there are already uploaded files remove them
      if (this.uploadedFiles.length > 0) this.uploadedFiles = []

      e.dataTransfer.files.forEach((element) =>
        this.uploadedFiles.push(element)
      )
    },
    submit() {
      // If there aren't any files to be uploaded throw error
      if (!this.uploadedFiles.length > 0) {
        this.$store.dispatch('addNotification', {
          message: 'There are no files to upload',
          colour: 'error'
        })
      } else {
        // Send uploaded files to parent component
        this.$emit('filesUploaded', this.uploadedFiles)
        // Close the dialog box
        this.closeDialog()
      }
    }
  }
}
</script>
