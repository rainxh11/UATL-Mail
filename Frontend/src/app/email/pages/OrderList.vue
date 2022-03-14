<template>
  <v-card class="d-flex flex-grow-1 flex-column" :loading="loading">
    <v-card-title >
      <v-row no-gutters dense>
        <v-col
          cols="12"
          lg="8"
          md="11"
          sm="11"
          xs="11"
        >
          <v-text-field
            v-model="searchQuery"
            append-icon="fa-regular fa-magnifying-glass"
            class="flex-grow-1 mr-md-2"
            outlined
            dense
            hide-details
            clearable
            :placeholder="$t('common.search')"
          ></v-text-field>
        </v-col>
        <v-col cols="1">
          <v-btn
            v-show="selected.length === 0"
            icon
            :loading="loading"
            @click="page = 1; refresh({ page: page, limit: pageSize })"
          >
            <v-icon>fa-regular fa-refresh</v-icon>
          </v-btn>
        </v-col>
        <v-col
          cols="12"
          lg="3"
          md="12"
          sm="12"
        >
          <v-pagination
            v-model="page"
            circle
            dense
            color="primary"
            :length="orders.PageCount"
            :total-visible="5"
            @input="refresh({ page: page, limit: pageSize })"
          ></v-pagination>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider/>
    <v-list
      v-for="group in orders.Data
        .AsEnumerable()
        .GroupBy(x => new Date(x.SentOn ? x.SentOn : x.CreatedOn) .toString('yyyy/MM/dd'), x => x)
        .ToArray()"
      :key="group"
      class="px-2"
    >
      <v-subheader>
        <v-list-item-action class="d-flex flex-row align-center">
          <v-list-item-action class="d-flex flex-row align-center">
            <v-icon>fa-regular fa-calendar-day</v-icon>
          </v-list-item-action>
          <v-list-item-action-text>
            <span class="text--primary text-h6">
              {{ new Date(group.key) | formatDate('dddd, DD MMMM yyyy') | uppercase }}
            </span>
          </v-list-item-action-text>
        </v-list-item-action></v-subheader>
      <v-divider/>
      <template v-for="item in group.values()">
        <v-list-item
          :key="item.ID"
          link
          two-line
          :class="{
            'v-list-item--active warning--text': item.Reviewed && !item.Approved,
            'v-list-item--active success--text': item.Approved,
            'v-list-item--active primary--text': selected.indexOf(item.ID) !== -1
          }"
          @click.stop
        >

          <v-list-item-avatar v-if="item.From" class="d-flex flex-row ma-1">
            <v-img :src="getAvatar(item.From.ID)" lazy-src="/images/avatars/generic.jpg"/>
          </v-list-item-avatar>        

          <v-list-item-action class="align-center d-flex flex-row" >
            <v-list-item-action-text v-if="item.From" class="px-1 text-lg-body-1">
              {{ $t('email.from') }}<b>{{ item.From.Name }}</b>
              <div v-if="item.From.Description" class="px-1">({{ item.From.Description }})</div>
            </v-list-item-action-text>
            <v-list-item-avatar v-if="item.To" class="d-flex flex-row ma-1">
              <v-img :src="getAvatar(item.To.ID)" lazy-src="/images/avatars/generic.jpg"/>
            </v-list-item-avatar>
            <v-list-item-action-text v-if="item.To" class="px-1 text-lg-body-1">
              {{ $t('email.to') }}<b>{{ item.To.Name }}</b>
              <div v-if="item.To.Description" class="px-1">({{ item.To.Description }})</div>
            </v-list-item-action-text>
            <v-list-item-action-text 
              
              v-else-if="item.Recipients.length > 1"
              class="px-1 text-lg-body-1"
            >
              <group-avatar :max="10" :avatars="item.Recipients.map(x => getAvatar(x.ID))" />
              <div v-for="recipient in item.Recipients" :key="recipient.ID" >{{ $t('email.to') }}<b>
                {{ recipient.Name }}</b>
                <span v-if="recipient.Description" class="px-1">({{ recipient.Description }})</span></div>
            </v-list-item-action-text>
          </v-list-item-action>

          <v-list-item-content class="pa-2 align-rigth d-flex flex-row">
            <v-list-item-title
              :class="{ 
                'text-h6 black--text': !$vuetify.theme.dark, 
                'text-h6 white--text': $vuetify.theme.dark
              }"
              v-html="$options.filters.highlight(item.Subject, search)"
            >
            </v-list-item-title>
          </v-list-item-content>

          <v-list-item-action v-if="!item.Approved">
            <v-btn v-if="!item.Reviewed" rounded class="blue mx-2 white--text" @click="review(item)">
              {{ $t('email.markReceived') }}
            </v-btn>
            <v-chip
              v-else
              outlined
              class="mx-2"
              color="blue"
            >
              {{ $t('email.orderReceived') }}
              <v-icon small class="px-1">fa-check</v-icon>
            </v-chip>
          </v-list-item-action>

          <v-list-item-action v-if="item.Reviewed">
            <v-btn v-if="!item.Approved" rounded class="success" @click="approve(item)">
              {{ $t('email.approve') }}
            </v-btn>
            <v-chip
              v-else
              outlined
              color="success"
            >
              {{ $t('email.approved') }}
              <v-icon small class="px-1">fa-check</v-icon>
            </v-chip>
          </v-list-item-action>

          <v-list-item-action>
            <v-list-item-action-text>
              <v-tooltip bottom>
                <template v-slot:activator="{ on, attrs }">
                  <span v-bind="attrs" class="text--primary text-lg-body-2" v-on="on">
                    {{ item.SentOn | formatTimeAgo }}
                    <v-icon color="primary" x-small>fa-regular fa-clock</v-icon>
                  </span>
                </template>
                <span >{{ item.SentOn | formatDate('HH:mm | DD MMMM YYYY') }}</span>
              </v-tooltip>
            </v-list-item-action-text>
          </v-list-item-action>
        </v-list-item>
      </template>
      <template v-if="orders.Data.length === 0">
        <div class="px-1 py-6 text-center">{{ $t('email.emptyList') }}</div>
      </template>
    </v-list>
  </v-card>
</template>

<script>
import { getAllOrders, searchOrders, reviewOrders, approveOrders } from '@/api/orders'
import { GroupAvatar } from 'vue-group-avatar'
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators'

export default {
  components: {
    GroupAvatar
  },
  data() {
    return {
      loading: false,
      search: '',
      searchQuery: '',
      selected: [],
      headers: [
        { text: this.$t('email.headers.subject'), align: 'left', value: 'Subject' },
        { text: this.$t('email.headers.sender'), align: 'left', value: 'From' },
        { text: this.$t('email.headers.destination'), align: 'left', value: 'To' },
        { text: this.$t('email.headers.status'), value: 'Approved' },
        { text: this.$t('email.headers.sentOn'), value: 'SentOn' },
        { text: '', sortable: false, align: 'right', value: 'action' }
      ],
      orders: [],
      starred: [],
      page:1,
      pageSize: 10
    }
  },
  watch: {
    page(val) {
      this.refresh()
    },
    searchQuery(val) {
      this.search = val
    }
  },
  subscriptions() {
    return {
      searchObservable: this.$watchAsObservable('search')
        .pipe(debounceTime(1000))
        .subscribe(val => {
          if (!val.newValue) this.search = ''
          this.refresh()
        })
    }
  },
  async created() {
    this.$mailHub.on('refresh_orders', (x) => {
      if (this.search.length > 0) {
        this.refresh()
      }
    })
    try {
      if (this.$mailHub.state === 'Disconnected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  mounted() {
    this.refresh()
  },
  beforeDestroy() {
    this.$observables.searchObservable.unsubscribe()
  },
  methods: {
    refresh(params =  { page: this.page, limit: this.pageSize }) {
      if (this.search.length > 0) {
        this.searchOrders(this.search, params)
      } else {
        this.getOrders(params)
      }
    },
    getAvatar(id) {
      return `${this.$apiHost}/api/v1/account/${id}/avatar`
    },
    getOrders(params) {
      this.loading = true
      getAllOrders(params)
        .then(res => {
          this.orders = res.data
          console.log(res.data)
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    searchOrders(search, params) {
      this.loading = true
      searchOrders(search, params)
        .then(res => {
          this.orders = res.data
        })
        .catch(err => console.log(err))
        .finally(() => {
          this.loading = false
          console.log(this.loading, 'loading')
        })
    },
    approve(item) {
      approveOrders(item.GroupId)
        .then(res => this.refresh())
        .catch(err => console.log(err))
    },
    review(item) {
      reviewOrders(item.GroupId)
        .then(res => this.refresh())
        .catch(err => console.log(err))
    }
  }
}
</script>
<style scoped>
.highlight {
  font-weight: bold;
  background-color: orange;
}
</style>