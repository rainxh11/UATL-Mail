<template>
  <div class="align-center">
    <div v-html="body" />
    <div v-if="hashTags.length > 0" class="pa-2">
      <v-chip
        v-for="tag in hashTags"
        :key="tag"
        class="font-italic"
        :dark="getUniqueColor(tag).dark"
        :light="getUniqueColor(tag).light"
        :color="getUniqueColor(tag).color"
      >
        {{ tag }}
      </v-chip>
    </div>
    <v-divider/>

    <viewer :images="images">
      <v-tooltip v-for="(src, index) in images" :key="src" bottom>
        <template v-slot:activator="{ on, attrs }">
          <img
            v-bind="attrs"
            height="200"
            :src="src.url"
            class="cursor-pointer column-flex"
            v-on="on"
            @click="show(index)"
          />
        </template>
        <span>{{ src.name }}</span>
      </v-tooltip>
    </viewer>
    <v-container v-if="files.length > 0">
      <v-subheader class="text-h6">{{ $t('email.attachments') }}</v-subheader>
      <v-virtual-scroll      
        :items="$enumerable(files).OrderByDescending(x => x.FileSize).ToArray()"
        :max-height="filesHeight"
        item-height="30"
      >
        <template v-slot:default="{ item }">
          <v-list-item :key="item.ID">
            <v-list-item-icon> 
              <v-icon small :color="getIcon(item).color">{{ getIcon(item).icon }}</v-icon>
            </v-list-item-icon>
            <v-list-item-action v-if="getIcon(item).type === 'audio'">
              <v-btn icon color="primary" @click="playAudio(item)">
                <v-icon>{{ getPlayIcon(item) }}</v-icon>
              </v-btn>
            </v-list-item-action>
            <v-list-item-content>
              <v-list-item-title>
                <a :href="getFileUrl(item)"> {{ item.Name }}</a>
              </v-list-item-title>
            </v-list-item-content>

            <v-list-item-action-text>
              <span class="ml-1 text-lg-body-1 text--secondary">
                {{ item.FileSize | formatByte }}
              </span>
            </v-list-item-action-text>
            <v-list-item-action v-if="canEdit">
              <v-btn icon @click.stop="removeFile(item.ID)">
                <v-icon color="red">fa-solid fa-circle-xmark</v-icon>
              </v-btn>
            </v-list-item-action>
          </v-list-item>

          <v-divider></v-divider>
        </template>
      </v-virtual-scroll>
    </v-container> 
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
import seedColor from 'seed-color'
import isDarkColor from 'is-dark-color'
import { getMimeIcon } from '../../../plugins/mimeToIcon'

export default {
  name: 'BodyContent',
  props: {
    // Input label
    body: {
      type: String,
      default: '',
      required: true
    },
    files: {
      type: Array,
      default: () => []
    },
    hashTags: {
      type: Array,
      default: () => []
    },
    canEdit: {
      type: Boolean,
      default: false
    },
    filesHeight: {
      type: Number,
      default: 300
    }
  },
  data() {
    return {
      audio: {
        id: null,
        handle: null
      }
    }
  },
  computed: {
    images() {
      return this.$enumerable(this.files)
        .Where(x => x.ContentType.includes('image'))
        .Select(x => {
          return { 
            url: `${this.$apiHost}/api/v1/attachment/${x.ID}`,
            name: x.Name
          }})
        .ToArray()
    }
  },
  watch: {
  },
  mounted() {
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    show(index) {
      this.$viewerApi({
        images: this.images
          .map(x => x.url),
        options: {
          initialViewIndex: index
        }     
      })
    },
    isHashTag(val) {
      if (!val) return false
      if (val === '|') return false
      const hashTagRegex = /(#+[a-zA-Z0-9(_)(\-)]{1,})/i
      const whiteSpaceRegex = /[\s]/i
      const match = val.match(hashTagRegex) !== null && val.match(whiteSpaceRegex) !== null
      return !match
    },
    getUniqueColor(val) {
      return {
        color: seedColor(val).toHex(),
        dark: isDarkColor(seedColor(val).toHex()) && !this.$vuetify.theme.dark,
        light: !isDarkColor(seedColor(val).toHex()) && this.$vuetify.theme.dark
      }
    },
    removeFile(id) {
      this.files = this.$enumerable(this.files)
        .Where(x => x.ID !== id)
        .ToArray()
    },
    getFileUrl(file) {
      return `${this.$apiHost}/api/v1/attachment/${file.ID}`
    },
    getIcon(file) {
      const icon = getMimeIcon(file.Extension)
      //return icon ? { icon: 'fa-file', color: 'orange' } : icon
      return icon
    },
    playAudio(file) {
      const url = this.getFileUrl(file)
      if (this.audio.handle) {
        this.audio.handle.pause()
        this.audio.handle.currentTime = 0
      }
      this.audio.handle = new Audio(url)
      this.audio.id = file.ID
      this.audio.handle.play()
    },
    getPlayIcon(file) {
      if (this.audio.id !== file.ID) return 'fa-circle-play'
      if (this.audio.handle.currentTime > 0) {
        if (this.audio.handle.paused) return 'fa-circle-play'
        else return 'fa-circle-pause'
      } else return 'fa-circle-play'
    }
  }
}
</script>
