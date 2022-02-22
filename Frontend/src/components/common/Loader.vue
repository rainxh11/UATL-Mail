<template>
  <v-dialog
    hide-overlay
    persistent
    :width="300"
    @keydown.esc.prevent="$emit('close')"
  >
    <v-card
      :color="color"
      dark
    >
      <v-card-text>
        {{ message }}
        <v-progress-linear
          indeterminate
          :color="progressColor"
          class="mb-0"
        ></v-progress-linear>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  name: 'Loader',
  props: {
    progressColor:{
      type: String,
      default: 'white'
    },
    color: {
      type: String,
      default: 'primary'
    },
    message: {
      type: String,
      required: true
    },
    width: {
      type: Number,
      default: 300
    }
  }
}
</script>

<style scoped>

</style>
