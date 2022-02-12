<template>
  <v-row data-app="true">
    <v-col>
      <v-card>
        <v-container fluid>
          <v-row cols="12" sm="9" offset-sm="1">
            <v-col
              v-for="(study, index) in studies"
              :key="index"
              class="d-flex child-flex"
              cols="4"
            >
              <v-card flat tile class="d-flex">
                <v-img
                  :src="study.image"
                  aspect-ratio="1"
                  class="grey lighten-2"
                  @click.stop=""
                >
                  <template v-slot:placeholder>
                    <v-row
                      class="fill-height ma-0"
                      align="center"
                      justify="center"
                    >
                      <v-progress-circular
                        indeterminate
                        color="grey lighten-5"
                      ></v-progress-circular>
                    </v-row>
                  </template>
                </v-img>
              </v-card>
            </v-col>
          </v-row>
        </v-container>
      </v-card>
    </v-col>
  </v-row>
</template>

<script>
export default {
  props: ['studies'],
  data() {
    return {
    }
  }
}
</script>
