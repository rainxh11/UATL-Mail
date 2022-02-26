<template>
  <v-autocomplete
    v-model="model"
    :items="items"
    :filter="filter"
    :search-input.sync="search"
    :label="label"
    :loading="searchLoading"
    item-value="ID"
    item-text="Name"
    :prepend-inner-icon="model.length > 1 ? 'fa-users' : 'fa-user'"
    outlined
    clearable
    clear-icon="fa-circle-xmark"
    multiple
    :rules="rules.recipients"
  >
    <template v-slot:append-outer>
      <slot></slot>
    </template>

    <template v-slot:selection="data">
      <v-chip
        v-bind="data.attrs"
        :input-value="data.selected"
        close
        @click="data.select"
        @click:close="remove(data.item)"
      >
        <v-avatar left>
          <v-img :src="avatar(data.item.Avatar)"></v-img>
        </v-avatar>
        {{ data.item.Name }}
      </v-chip>
    </template>

    <template v-slot:item="{ index, item }">
      <v-list-item-avatar>
        <v-img :src="avatar(item.Avatar)" />
      </v-list-item-avatar>

      <v-list-item-content>
        <v-list-item-title>{{ item.Name }}</v-list-item-title>
        <v-list-item-subtitle>{{ item.Description ? item.Description : '' }}</v-list-item-subtitle>
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
import { searchRecipients } from '@/api/users'
import { debounceTime, distinctUntilChanged, pluck, map } from 'rxjs/operators'
import { mapGetters } from 'vuex'

export default {
  props: {
    // Input label
    label: {
      type: String,
      default: ''
    },
    // Email addresses
    addresses: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      model: [],
      search: '',
      searchLoading: false,
      items: [],
      searchObservable: null,
      rules: {
        recipients: [
          (value) => value.length > 0 || this.$t('rules.recipientsRequired')
        ]
      }
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
        this.searchRecipients(val)
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
      return item.UserName.includes(queryText) || item.Name.includes(queryText)
    },
    remove(val) {
      this.model = this.$enumerable(this.model)
        .Where(x => x !== val.ID)
        .ToArray()
    },
    searchRecipients(val) {
      this.searchLoading = true
      searchRecipients(val, this.getToken())
        .then(res => {
          this.items.length = 0
          this.items = res.data.Data
        })
        .catch(err => console.log(err))
        .finally(() => this.searchLoading = false)
    }
  }
}
</script>
