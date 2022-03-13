<template>
  <v-autocomplete
    v-model="model"
    :items="tags"
    :filter="filter"
    :search-input.sync="search"
    :label="label"
    :loading="searchLoading"
    item-value="item"
    item-text="item"
    prepend-inner-icon="fa-tags"
    dense
    flat
    solo
    clearable
    clear-icon="fa-circle-xmark"
    multiple
  >
    <template v-slot:append-outer>
      <slot></slot>
    </template>

    <template v-slot:selection="data">
      <v-chip
        v-bind="data.attrs"
        :input-value="data.selected"
        close
        :dark="getUniqueColor(data.item).dark"
        :light="getUniqueColor(data.item).light"
        :color="getUniqueColor(data.item).color"
        @click="data.select"
        @click:close="remove(data.item)"
      >
        {{ data.item }}
      </v-chip>
    </template>

    <template v-slot:item="{ item }">
      <v-list-item-content>
        <v-list-item-title>{{ item }}</v-list-item-title>
      </v-list-item-content>
    </template>
  </v-autocomplete>
</template>

<script>
/*
|---------------------------------------------------------------------
| Email Input Component
|---------------------------------------------------------------------
|
| Add and remove emails input
|
*/
import { searchTags } from '@/api/mails'
import { debounceTime, distinctUntilChanged, map, pluck } from 'rxjs/operators'
import { mapGetters } from 'vuex'
import seedColor from 'seed-color'
import isDarkColor from 'is-dark-color'

export default {
  name: 'HashTagInput',
  props: {
    // Input label
    label: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      model: [],
      search: '',
      searchLoading: false,
      tags: [],
      searchObservable: null
    }
  },
  watch: {
    model(val) {
      this.$emit('change', val)
    }
  },
  mounted() {
    this.searchObservable = this.$watchAsObservable('search')
      .pipe(
        distinctUntilChanged((p, c) => p === c),
        debounceTime(500),
        pluck('newValue'),
        map(x => x ?? '')
      )
      .subscribe(val => {
        this.searchTags(val)
      })
  },
  beforeDestroy() {
    this.searchObservable.unsubscribe()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    avatar(val) {
      return `${this.$apiHost}/api/v1${val}`
    },
    filter (item, queryText, itemText) {
      if (!item) return false
      if (!queryText) return true
      return item.includes(queryText) || this.model.some(x => x.includes(item))
    },
    remove(val) {
      this.model = this.$enumerable(this.model)
        .Where(x => x !== val)
        .ToArray()
    },
    searchTags(val) {
      val = val ?? '|'
      val = val.length === 0 ? '|' : val
      this.searchLoading = true
      val = val.replaceAll('#','')
      searchTags(val)
        .then(res => {
          this.tags.length = 0
          let tags = this.$enumerable(res.data)
          if (this.isHashTag(`#${val}`)) {
            tags = tags.Concat([`#${val}`])
          }
          this.tags = tags
            .Concat(this.model)
            .Distinct()
            .Where(x => this.isHashTag(x))
            .Where(x => x !== '#|')
            .ToArray()
        })
        .catch(err => console.log(err))
        .finally(() => this.searchLoading = false)
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
    }
  }
}
</script>
