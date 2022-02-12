<template>
  <div class="d-flex flex-column flex-grow-1">
    <div class="d-flex align-center py-3">
      <div>
        <div class="display-1">{{ $t('menu.studyPage') }}</div>
        <v-breadcrumbs :items="breadcrumbs" class="pa-0 py-2"></v-breadcrumbs>
      </div>
      <v-spacer></v-spacer>
    </div>
    <v-card :loading="isloadingTable">
      <v-card-title>
        <v-row dense >
          <v-col cols="12" class="align-center text--right d-flex">
            <v-menu offset-y left>
              <template v-slot:activator="{ on }">
                <transition name="slide-fade" mode="out-in">
                  <v-btn v-show="selectedItem.length > 0" class="mr-3 orange" v-on="on">
                    Actions
                    <v-icon right>mdi-menu-down</v-icon>
                  </v-btn>
                </transition>
              </template>
              <v-list dense>
                <v-list-item :disabled="this.getUserInfo().role !== 'admin'" @click="cancelStudies(selectedItem)" >
                  <v-list-item-title >Annuler l'Examen(s)</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
            <v-text-field
              v-model="searchValue"
              append-icon="mdi-magnify"
              class="flex-grow-1 mr-md-2"
              outlined
              hide-details
              dense
              clearable
              placeholder="Recherche"
            ></v-text-field>
            <v-btn
              :loading="isLoadingRefreshBtn"
              icon
              small
              class="ml-2"
              @click="refresh()"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>

          </v-col>
        </v-row>
      </v-card-title>

      <v-data-table
        v-model="selectedItem"
        :search="searchQuery"
        :headers="headers"
        :items="itemTable"
        sort-by="createdAt"
        sort-desc
        show-select
        class="flex-grow-1 font-weight-bold"
        :items-per-page="10"
        :footer-props="{
          itemsPerPageOptions: [5,10,20,50,100],
          showCurrentPage : true,
          showFirstLastPage : true,

        }"
      >

        <template v-slot:item.id="{ item }">
          <v-chip :style="'background-color: ' + itemRowBackground(item)">
            <div :class="getColorByBgColor(itemRowBackground(item))"> <copy-lable :text="item.id"></copy-lable></div>
          </v-chip>

        </template>

        <template v-slot:item.client="{ item }">
          <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
              <span v-bind="attrs" class="font-weight-black" v-on="on">
                {{ item.client | uppercase() }}
              </span>
              <div class=" font-italic">{{ ' ' + calcAgeFrench(new Date(item.clientObj.birthdate)) }} </div>
            </template>
            <div v-if="item.doctor">Médecin Traitant:<Br/><span class="font-weight-black text-lg-h6">{{ item.doctor }}</span></div>
            <div>N° Tél:<Br/><span class="font-weight-black text-lg-h6">{{ item.clientObj.phoneNumber }}</span></div>
          </v-tooltip>
        </template>

        <template v-slot:item.examType="{ item }">
          <div class="font-weight-black text-h6">{{ item.modality }} </div>
          <span > {{ item.examType }}</span>

          <v-tooltip v-if="item.product.length > 0" bottom>
            <template v-slot:activator="{ on, attrs }">
              <v-icon color="purple" v-bind="attrs" v-on="on">
                mdi-medical-bag
              </v-icon>
            </template>
            <span v-for="(it , i) in item.product" :key="i">{{
              it.name + ' X ' + it.quantityP + ' : ' + (it.quantityP * it.price ) | formatCurrency
            }} <br></span>
          </v-tooltip>

        </template>

        <template v-slot:item.statusPayment="{ item }">
          <div v-if="item.statusPayment === 'notPaid'" class="error--text">
            <v-icon small color="error">mdi-circle-medium</v-icon>
            <span>Non payé</span>
          </div>
          <div v-if="item.statusPayment === 'debt'" class="orange--text">
            <v-icon small color="orange">mdi-circle-medium</v-icon>
            <span>Dette</span>
          </div>
          <div v-if="item.statusPayment === 'paid'" class="success--text">
            <v-icon small color="success">mdi-circle-medium</v-icon>
            <span>Payé</span>
          </div>
        </template>

        <template v-slot:item.statusStudy="{ item }">
          <v-chip :class="getClassChip(item.statusStudy)">
            {{ $t('statusStudy.'+item.statusStudy) }}
          </v-chip>

        </template>

        <template v-slot:item.price="{ item }">
          <div v-if="!checkItem(item)">
            <v-tooltip bottom>
              <template v-slot:activator="{ on, attrs }">
                <v-icon v-if="item.conv !== 'normal'" color="orange">mdi-star</v-icon>
                <span v-if="!item.discount" v-bind="attrs" v-on="on" >{{ (item.group)? calcPriceGroup(item.group) : item.price | formatCurrency }}</span>
                <span v-if="item.discount" :class="item.convPrice ? 'orange--text' : 'red--text'" v-bind="attrs" v-on="on" >{{ ((item.group)? calcPriceGroup(item.group) - ((item.discount)? item.discount * itemTable.filter((it) => it.group === item.group ).length : 0) : item.price - ((item.discount )? item.discount : 0)) | formatCurrency }}</span>
              </template>
              <span >
                Prix : <span class="font-weight-black">{{ (item.group)? calcPriceGroup(item.group) : item.price | formatCurrency }}</span>
                <br>
                Remise : <span class="font-weight-black">{{ (item.group)? (item.discount)? item.discount * itemTable.filter((it) => it.group === item.group ).length : 0 : (item.discount)? item.discount : 0 | formatCurrency }}</span>
                <br>
                Remise de Convention : <span class="font-weight-black">{{ (item.group)? calcPriceGroupConv(item.group) : item.convPrice | formatCurrency }}</span>
              </span>
              <div v-if="item.conv !== 'normal'">Convention : <span class="font-weight-black">{{ item.conv.toUpperCase() }}</span></div>
            </v-tooltip>
          </div>
          <!--
          <div v-else><v-icon v-if="item.conv !== 'normal'" color="orange">mdi-star</v-icon></div>
-->
        </template>
        <template v-slot:item.createdAt="{ item }">
          <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
              <v-chip color="black" dark v-bind="attrs" v-on="on">
                {{ item.createdAt | formatDate('DD-MM-YYYY | HH:mm') }} <Br/>
              </v-chip>
            </template>
            <div v-if="item.paidAt" >Payée le : <Br/>
              <span class="font-weight-black text-lg-h6">{{ item.paidAt | formatDate('DD-MM-YYYY | HH:mm') }}</span>
            </div>
          </v-tooltip>

        </template>

        <template v-slot:item.createdBy="{ item }">
          <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
              <span v-bind="attrs" v-on="on" >{{ item.createdBy.name }}</span>
            </template>
            <div>Crée par : <Br/><span class="font-weight-black">{{ item.createdBy.name }} ( {{ item.createdBy.email }}) </span></div>
            <div v-if="item.paidBy" >Payée par : <Br/><span class="font-weight-black">{{ item.paidBy.name }} ({{ item.paidBy.email }}) </span></div>
          </v-tooltip>
        </template>

        <template v-if="$store.getters['auth/getUserInfo'].role === 'admin'" v-slot:item.openstudy="{ item }">
          <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
              <v-btn
                v-if="$store.getters['auth/getUserInfo'].role === 'admin'"
                v-bind="attrs"
                icon
                v-on="on"
                @click="openInNewTab(item)"
              > <v-icon>mdi-open-in-new</v-icon></v-btn>
            </template>
            <span>Gérer le Compte-Rendu</span>
          </v-tooltip>
        </template>
        <template v-if="$store.getters['auth/getUserInfo'].role === 'admin'" v-slot:item.opendocument="{ item }">
          <v-tooltip bottom >
            <template v-slot:activator="{ on, attrs }">
              <v-btn
                v-bind="attrs"
                icon
                color="blue"
                v-on="on"
                @click="openDocument(item.id)"
              > <v-icon>fa-file-word</v-icon></v-btn>
            </template>
            <span>Ouvrir l'Examen dans Word</span>
          </v-tooltip>
        </template>

        <template v-slot:item.action="{ item }">
          <v-tooltip v-if="item.statusStudy !== 'canceled'" bottom >
            <template v-slot:activator="{ on, attrs }">
              <v-btn
                v-if=" !checkItem(item) && item.statusStudy === 'complete'"
                v-bind="attrs"
                icon
                v-on="on"
                @click="updateStudy(item , 'delivered')"
              > <v-icon>mdi-hand</v-icon></v-btn>
            </template>
            <span>Remettre l'étude au client </span>
          </v-tooltip>
          <v-tooltip
            v-if=" !checkItem(item) && item.statusPayment === 'notPaid' && item.statusStudy !== 'canceled'"
            bottom
          >
            <template v-slot:activator="{ on, attrs }">
              <v-btn
                v-bind="attrs"
                icon
                v-on="on"
                @click="openPayDialog(item)"
              > <v-icon>mdi-cash-multiple</v-icon></v-btn>
            </template>
            <span>Gérer le Paiment</span>
          </v-tooltip>
          <v-tooltip
            v-if="item.statusStudy !== 'canceled'"
            bottom
          >
            <template v-slot:activator="{ on, attrs }">

              <v-btn
                v-bind="attrs"
                icon
                v-on="on"
                @click="printLargeReceipt(item.id,(item.statusPayment === 'paid') ? 'Payé' : 'Dette',{
                  ...item,
                  client : item
                })"
              > <v-icon color="orange">mdi-printer</v-icon>
              </v-btn>
            </template>
            <span>Imprimer Pour (KHALED)</span>
          </v-tooltip>
          <v-tooltip
            v-if=" !checkItem(item) && item.statusPayment !== 'notPaid' && item.statusStudy !== 'canceled'"
            bottom
          >
            <template v-slot:activator="{ on, attrs }">

              <v-btn
                v-bind="attrs"
                icon
                v-on="on"
                @click="printReceipt(item.id,(item.statusPayment === 'paid') ? 'Payé' : 'Dette',{
                  ...item,
                  client : item.client
                })"
              > <v-icon>mdi-printer</v-icon>
              </v-btn>
            </template>
            <span>Imprimer Facture</span>
          </v-tooltip>
        </template>
      </v-data-table>
    </v-card>
    <v-dialog v-model="showCreateDialog" :max-width="(groupDialogStudy.show) ? 1000 : 500">
      <v-card style="overflow-x: hidden; " :loading="isLoadingDialog">
        <v-card-title class="title">Ajouter une tache <v-spacer></v-spacer> <v-btn icon @click="showCreateDialog = false"> <v-icon>mdi-close</v-icon></v-btn></v-card-title>
        <v-divider></v-divider>
        <v-row no-gutters>
          <v-col>
            <div >
              <v-list-item v-if="client.firstName !== undefined">
                <v-list-item-avatar rounded color="black">
                  <v-icon dark> {{ 'mdi-alpha-' + client.familyName[0].toLocaleLowerCase() + '-box-outline' }} </v-icon>
                </v-list-item-avatar>
                <v-list-item-content >
                  <v-list-item-title > {{ client.familyName + ' ' + client.firstName }} </v-list-item-title>
                  <v-list-item-subtitle class="font-weight-bold"> {{ client.birthdate | formatDate('DD-MM-YYYY') }}</v-list-item-subtitle>
                </v-list-item-content>
                <v-list-item-action>
                  <v-btn color="primary" fab small @click="openAddManyList()" >
                    <v-icon> mdi-plus-box-multiple</v-icon>
                  </v-btn>
                </v-list-item-action>
              </v-list-item>
            </div>
            <v-divider></v-divider>
            <div>
              <v-row class="px-3 pr-4">
                <v-text-field
                  v-if="!checkAutoComplete"
                  v-model="autoCompleteDoctor"
                  label="Medcin"
                  filled
                  maxlength="20"
                  counter="20"
                  autofocus
                  hide-details
                  class="px-2 py-1 search-field-2"
                ></v-text-field>

                <v-autocomplete
                  v-if="checkAutoComplete"
                  v-model="autoCompleteDoctor"
                  label="Medcin"
                  :items="autoCompleteDoctorItems"
                  item-text="name"
                  item-value="name"
                  outlined
                  maxlength="20"
                  counter="20"
                  autofocus
                  hide-details
                  class="px-2 py-1 search-field-2"
                ></v-autocomplete>

                <v-checkbox v-model="checkAutoComplete">
                </v-checkbox>

              </v-row>

            </div>

            <v-divider></v-divider>
            <div>
              <v-autocomplete
                ref="firstNameDialog"
                v-model="autoCompleteModality"
                label="Group"
                :items="autoCompleteModalityItems"
                item-text="name"
                outlined
                return-object
                maxlength="20"
                counter="20"
                hide-details
                class="px-2 py-1 search-field-2"
                @change="getAllTypeExam"
              ></v-autocomplete>

            </div>

            <v-divider></v-divider>
            <div>
              <v-autocomplete
                ref="firstNameDialog"
                v-model="autoCompleteTypeExam"
                label="Type"
                :items="autoCompleteTypeExamItems"
                item-text="name"
                return-object
                :disabled="autoCompleteTypeExamDisable"
                outlined
                maxlength="20"
                counter="20"
                hide-details
                class="px-2 py-1 search-field-2"
                @change="updateConventionList()"
              >
                <template v-slot:item="{ item }">

                  <v-list-item-content>
                    <v-list-item-title>
                      {{ item.name }}

                    </v-list-item-title>
                    <v-list-item-subtitle>
                      {{ item.price | formatCurrency }}
                    </v-list-item-subtitle>
                  </v-list-item-content>

                </template>

              </v-autocomplete>

            </div>
            <v-divider></v-divider>
            <div>
              <v-row class="px-3 pr-4">

                <v-autocomplete
                  v-model="autoCompleteConv"
                  :disabled="!checkConvAutoComplete"
                  label="Convention"
                  :items="autoCompleteConvItems"
                  item-text="convention.name"
                  return-object
                  outlined
                  maxlength="20"
                  counter="20"
                  hide-details
                  class="px-2 py-1 search-field-2"
                >
                  <template v-slot:item="{ item }">

                    <v-list-item-content>
                      <v-list-item-title>
                        {{ item.convention.name }}

                      </v-list-item-title>
                      <v-list-item-subtitle>
                        {{ item.clientPrice | formatCurrency }}
                      </v-list-item-subtitle>
                    </v-list-item-content>

                  </template>

                </v-autocomplete>

                <v-checkbox v-model="checkConvAutoComplete">
                </v-checkbox>

              </v-row>

            </div>

            <v-divider></v-divider>

            <v-div>
              <v-select
                v-model="productSelected"
                outlined
                label="Produit"
                item-text="name"
                return-object
                class="px-2 pt-2"
                :items="productList"
                @change="changeProduct"
              ></v-select>
            </v-div>
            <div v-if="selectItemsProduct.length > 0">
              <div class="px-2">
                <h3 > List </h3>
                <v-divider></v-divider>
              </div>
              <v-item-group class="pa-2" >

                <v-list-item v-for="(item, i) in selectItemsProduct" :key="i">
                  <v-list-item-content >
                    <v-list-item-title > {{ `${item.name} x ${item.quantityP}` }} </v-list-item-title>
                    <v-list-item-subtitle class="font-weight-bold"> {{ item.price | formatCurrency }} </v-list-item-subtitle>
                  </v-list-item-content>
                  <v-list-item-action-text>
                    <v-btn
                      class="mr-1"
                      fab
                      x-small
                      color="blue"
                      @click="item.quantityP++"
                    >
                      <v-icon color="white"> mdi-plus</v-icon>
                    </v-btn>
                    <v-btn fab x-small color="blue" @click="(item.quantityP === 1)? selectItemsProduct.splice(i,1) : item.quantityP--">
                      <v-icon color="white"> mdi-minus</v-icon>
                    </v-btn>
                  </v-list-item-action-text>
                </v-list-item>
                <v-divider></v-divider>

              </v-item-group>
            </div>

          </v-col>
          <v-divider vertical/>
          <v-col v-if="groupDialogStudy.show">
            <v-toolbar
              color="blue"
              class="white--text"
            >

              <v-toolbar-title>List d'exam</v-toolbar-title>

              <v-spacer></v-spacer>

              <v-btn v-if="groupDialogStudy.selected !== undefined" icon @click="groupDialogStudy.studyList.splice(groupDialogStudy.selected,1)">
                <v-icon color="white">
                  mdi-delete
                </v-icon>
              </v-btn>

            </v-toolbar>
            <v-list-item-group
              v-model="groupDialogStudy.selected"
              active-class="pink--text"
            >
              <template
                v-for="(item, i) in groupDialogStudy.studyList"
              >
                <v-list-item :key="i">
                  <v-list-item-content >
                    <v-list-item-title > {{ item.modality }} </v-list-item-title>
                    <v-list-item-subtitle class="font-weight-bold"> {{ item.examType }} </v-list-item-subtitle>
                  </v-list-item-content>
                  <v-list-item-action-text>
                    {{ item.price | formatCurrency }}
                  </v-list-item-action-text>
                </v-list-item>
                <v-divider :key="i"></v-divider>
              </template>
            </v-list-item-group>
          </v-col>
        </v-row>

        <v-divider></v-divider>
        <v-card-actions>
          <v-btn color="error" @click="showCreateDialog = false">
            Annulé
          </v-btn>
          <v-spacer></v-spacer>

          <h3 class="mr-2">Prix : {{ calcPriceCreateDialog() | formatCurrency }}</h3>

          <v-btn v-if="!groupDialogStudy.show" color="primary" @click="createStudy()">
            Ajouter
          </v-btn>
          <v-btn
            v-if="groupDialogStudy.show"
            fab
            small
            color="primary"
            @click="addStudyToList()"
          >
            <v-icon>
              mdi-plus
            </v-icon>
          </v-btn>
          <v-btn
            v-if="groupDialogStudy.show"
            fab
            small
            color="success"
            @click="creatManyStudies()"
          >
            <v-icon>
              mdi-check
            </v-icon>
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog v-model="showPayDialog" max-width="600">
      <v-card style=" overflow: hidden;">
        <v-card-title class="title">{{ this.$t('common.payment') }} <v-spacer></v-spacer> <v-btn icon @click="showPayDialog = false"> <v-icon>mdi-close</v-icon></v-btn></v-card-title>
        <v-divider></v-divider>
        <v-row class="px-2 align-center justify-center">
          <v-radio-group
            v-model="radioPay"
            row
          >
            <v-radio
              :label="this.$t('tableStudies.normalPayment')"
              value="normal"
              color="success"
            ></v-radio>
            <v-radio
              :label="this.$t('tableStudies.debtPayment')"
              color="orange"
              value="dept"
            ></v-radio>
          </v-radio-group>

        </v-row>

        <v-divider v-if="radioPay === 'dept'"></v-divider>

        <div >
          <v-text-field
            v-if="radioPay === 'dept'"
            v-model="deptAmount"
            :rules="rules.debtAmount"
            :label="this.$t('tableStudies.initialPayment')"
            prepend-icon="mdi-cash"
            outlined
            minlength="1"
            class="px-2 py-1 search-field-2 text-uppercase"
            type="number"
            suffix="DA"
            @focus="$event.target.select()"
          >
          </v-text-field>
        </div>
        <div >
          <v-subheader>{{ this.$t('common.discount') }}</v-subheader>
          <v-row no-gutters>
            <v-col cols="*">
              <v-text-field
                v-model="discount.amount"
                :rules="rules.discount"
                :label="this.$t('common.discount')"
                prepend-icon="mdi-sale"
                outlined
                dense
                minlength="1"
                class="px-2 py-0 search-field-2 text-uppercase"
                type="number"
                suffix="DA"
                :disabled="!discount.enabled"
                @focus="$event.target.select()"
                @change="checkDiscount"
              >
              </v-text-field>
            </v-col>
            <v-col v-if="!discount.enabled" cols="3">
              <v-btn
                large
                color="primary"
                class="white--text"
                dark
                @click="dialog.token = true"
              >{{ this.$t('tableStudies.getPermission') }}</v-btn>
            </v-col>
          </v-row>
        </div>
        <v-divider></v-divider>

        <v-card-actions>
          <v-row class=" px-5 justify-center align-center">

            <h3 class="mr-2">{{ this.$t('common.totalPrice') + ':' }}</h3>
            <h2> {{ (itemToPay.group) ? calcPriceGroup(itemToPay.group) : calcPrice() | formatCurrency }}
            </h2>
            <v-spacer></v-spacer>

            <v-btn
              v-if="radioPay === 'normal'"
              x-large
              color="success"
              class="black--text"
              dark
              @click="updatePaymentStudy('paid')"
            >{{ this.$t('common.checkout') }}</v-btn>

            <v-btn
              v-if="radioPay === 'dept'"
              :disabled="((itemToPay.group)? parseInt(getPrice(true, itemToPay.group)) < deptAmount : calcPrice(false, null) < deptAmount) || !deptAmount"
              x-large
              color="warning"
              class="black--text"
              @click="updateStudyDebt()"
            >{{ this.$t('common.debt') }}</v-btn>

          </v-row>
        </v-card-actions>
      </v-card>
      <v-dialog
        v-model="dialog.token"
        max-width="500"
      >
        <v-card>
          <v-card-title class="title">
            {{ $t('tableStudies.discountPermission') }}
            <v-spacer></v-spacer>
            <v-btn icon @click="dialog.token = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </v-card-title>
          <v-row no-gutters class="px-2 justify-center align-center">
            <v-col cols="*">
              <v-text-field
                v-model="discount.token.value"
                v-mask="'######'"
                clearable
                class=" pt-3 text-md-h6 font-weight-bold text-uppercase"
                outlined
                dense
                :label="this.$t('setting.permissionPage.token')"
                @click:clear="discount.token = { value:'', show: false, correct: false }"
                @change="discount.token.show = false"
                @keydown.enter="verifyToken"
              />
            </v-col>
            <v-col cols="3" class="align-center justify-center">
              <v-btn
                :disabled="discount.token.value.length < 6 || !discount.token.value"
                :loading="loading.verify"
                color="primary"
                large
                class="font-weight-bold text-lg-h6 pa-2 ml-2"
                @change="discount.token.show = false"
                @click="verifyToken"
              >
                {{ this.$t('setting.permissionPage.verify') }}
              </v-btn>
            </v-col>
          </v-row>
          <v-row no-gutters class="px-2 pa-2 justify-center align-center">
            <v-chip
              v-if="discount.token.show"
              outlined
              dense
              class="text-md-h6 px-2 py-1 search-field-2 text-uppercase"
              :color="discount.token.correct ? 'success' : 'error'"
            >
              {{ discount.token.correct ? this.$t('common.correct') : this.$t('common.incorrect') }}
            </v-chip>
          </v-row>

        </v-card>
      </v-dialog>
    </v-dialog>
  </div>
</template>

<script>
import {
  createStudies,
  createStudy,
  getAllStudy,
  getOneStudy,
  printReceipt,
  printLargeReceipt,
  updateGroup,
  updateStudy,
  updateStudyDebt,
  updateStudyProduct,
  cancelStudies
} from '../../api/study'
import { getOneClient, printReceiptDebt } from '@/api/client'
import { printInvoice } from '../../api/print'

import { getAllModality } from '@/api/modality'
import { initTable } from '@/mixin/initTable'
import { mapActions, mapGetters } from 'vuex'

import CopyLable from '@/components/common/CopyLabel'
import { getAllExamTypeFilter } from '@/api/examtype'
import { createDoctor, getAllDoctor } from '@/api/doctor'
import { getAll2fa, verify2faToken } from '@/api/2fa'
import { getAllProduct } from '@/api/product'
import { debounceTime } from 'rxjs/operators'
import axios from 'axios'

export default {
  name: 'TableStudys',
  components: {
    CopyLable
  },
  mixins: [initTable],
  data() {
    return {

      groupSocketUpdate : 0,

      dialog: {
        token: false
      },
      loading:{
        verify: false
      },
      discount:{
        enabled: false,
        amount: 0,
        token: {
          correct: false,
          value: '',
          show:false
        }
      },
      // Top Page breadcrumbs
      breadcrumbs: [{
        text: this.$t('common.tasks'),
        disabled: true,
        href: '/studies'
      }, {
        text: this.$t('menu.studyPage'),
        disabled: false,
        href: '/studies'
      }],
      // Data table Variable -->
      rules: {
        debtAmount: [
          (value) => !!value || this.$t('setting.userDialog.required'),
          (v) => ((this.itemToPay.group) ? parseInt(this.getPrice(true, this.itemToPay.group)) >= v : this.getPrice(false, null) >= v) || this.$t('tableStudies.debtRules.maximum')
        ],
        discount: [
          (value) => !!value || this.$t('setting.userDialog.required')
        ]
      },
      headers: [
        { text: 'id', align: 'left', value: 'id', sortable: false },
        { text: 'Client', align: 'left', value: 'client' },
        //{ text: 'Group Exam', align: 'left', value: 'modality' },
        { text: 'Type Exam', align: 'left', value: 'examType' },
        { text: 'Etat des paiement', align: 'left', value: 'statusPayment' },
        { text: 'Prix', align: 'left', value: 'price' },
        { text: 'Etat des exam', align: 'left', value: 'statusStudy' },
        { text: 'Créé Le', align: 'left', value: 'createdAt' },
        { text: 'Créé par', align: 'left', value: 'createdBy' },
        { text: '', sortable: false, align: 'center', value: 'action' },
        { text: '', sortable: false, align: 'center', value: 'openstudy' },
        { text: '', sortable: false, align: 'center', value: 'opendocument' }

      ],
      // <--

      // Dialog Variable -->
      isLoadingDialog: false,
      showCreateDialog: false,
      studyInfo: {
        client: '',
        modality: '',
        examType: '',
        statusPayment: '',
        statusStudy: '',
        price: '',
        createdBy: ''
      },
      client : {},

      checkAutoComplete : true ,
      autoCompleteDoctor: '',
      autoCompleteDoctorItems: [],

      checkConvAutoComplete : false ,
      autoCompleteConv: '',
      autoCompleteConvItems: [],

      autoCompleteModality: {},
      autoCompleteModalityItems: [],

      autoCompleteTypeExam: {},
      autoCompleteTypeExamItems: [],
      autoCompleteTypeExamDisable: true,

      productList : [],
      productSelected : {},
      selectItemsProduct : [],

      groupId : '' ,
      groupDialogStudy : {
        show : false,
        studyList : [],
        selected : null
      },

      //<--

      // Pay Dialog Variable -->
      showPayDialog : '',
      radioPay : 'normal',
      deptAmount : 0,
      itemToPay : '',
      shownExams: [],
      // <--
      searchValue:''
    }
  },
  watch: {
    $route(to, from) {
      this.getAllDoctors()
      if (this.$route.query.id) {
        this.getStudies('?client=' + this.$route.query.id)
      }
      else if (this.$route.query.type) {
        if (this.$route.query.type === 'complete') {
          this.getStudies('?limit=200&statusStudy=complete&sort=-createdAt' )
        } else {
          this.getStudies('?limit=200&statusStudy=new&statusStudy=inProgress&sort=-createdAt' )
        }
      } else {
        this.getStudies('?limit=500&sort=-createdAt')
      }

      this.getAllModality()
    }
  },
  subscriptions() {
    return {
      search: this.$watchAsObservable('searchValue')
        .pipe(debounceTime(1000))
        .subscribe(({ old, newValue }) => this.searchQuery = newValue)
    }
  },
  created() {
    //this.$observables.search
  },
  mounted() {
    this.$socket.client.connect()

    this.$socket.client.on('newStudy' ,  (it) => {
      console.log(it)
      this.groupDialogStudy = 0
      this.refresh()
    }
    )
    this.$socket.client.on('updateStudy' , (it) => {
      let statusPayment = false
      let statusStudy = false

      Object.keys(it.fieldsUpdated).forEach( (iz) => {
        if (iz === 'statusPayment') {
          statusPayment = true
        }
        if (iz === 'statusStudy') {
          statusStudy = true
        }

      }
      )

      // eslint-disable-next-line array-callback-return
      this.itemTable.map((ir) => {
        if (it.id === parseInt(ir.id)) {
          if (statusPayment) {
            ir.statusPayment = it.fieldsUpdated.statusPayment
          }
          if (statusStudy) {
            ir.statusStudy = it.fieldsUpdated.statusStudy
          }
        }
      })
    })

    this.getAllProduct()
    this.getAllDoctors()
    if (this.$route.query.id) {
      this.getStudies('?client=' + this.$route.query.id)
    }
    else if (this.$route.query.type) {
      if (this.$route.query.type === 'new') {
        this.getStudies('?limit=200&statusStudy=new&statusStudy=inProgress&sort=-createdAt' )
      } else {
        this.getStudies('?limit=200&statusStudy=complete&sort=-createdAt' )

      }
    } else {
      this.getStudies('?limit=500&sort=-createdAt')
    }

    this.getAllModality()
    if (this.$route.query.add) {
      getOneClient(this.$route.query.add , this.getToken()).then( (res) => {
        this.client = res.data.data
        this.showSuccess('Success')
        this.showCreateDialog = true
        this.groupDialogStudy.selected = undefined
      }).catch( () => {

      })
    }

  },
  beforeDestroy() {
    this.$socket.client.disconnect()
    this.$observables.search.unsubscribe()
  },
  methods: {
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapGetters('app', ['getSocket']),
    ...mapActions('app', ['showSuccess', 'showError']),
    openDocument(id) {
      axios.post('http://127.0.0.1:5500/api/opendocument/' + id).then().catch((err) => {
        this.showError(err)
      })
    },
    openInNewTab(v) {
      const routeData = this.$router.resolve({ path : '/studies/' + v.id })

      window.open(routeData.href, '_blank')
    },
    getColorByBgColor(bgColor) {
      if (!bgColor) { return '' }

      return (parseInt(bgColor.replace('#', ''), 16) > 0xffffff / 2) ? 'font-weight-bold black--text' : 'font-weight-bold white--text'
    },
    calcAgeFrench(past) {
      const today = new Date()

      const diff = Math.floor(today.getTime() - past.getTime())

      const day = 1000 * 60 * 60 * 24

      const days = Math.floor(diff / day)
      const months = Math.floor(days / 31)
      const years = Math.floor(months / 12)

      return  (years !== 0 ? years + ' Ans' : (months !== 0) ? months + ' Mois' : days + ' Jour')
    },
    getAllDoctors() {
      getAllDoctor(this.getToken()).then((res) => {
        if (res.data.results) {
          this.autoCompleteDoctorItems = res.data.data
        }
      })

    },
    calcPriceCreateDialog() {
      if (this.groupDialogStudy.show) {
        let total = 0

        this.groupDialogStudy.studyList.forEach((it) => {
          total += it.price

        })

        return total
      }

      return ((this.checkConvAutoComplete) ? this.autoCompleteConv.clientPrice : this.autoCompleteTypeExam.price) + this.calcListProductPrice()
    },
    calcListProductPrice() {
      let total = 0

      this.selectItemsProduct.forEach((it) => {
        total = it.price * it.quantityP
      })

      return total
    },
    getPrice(group, v) {
      if (!group) {
        return parseInt(this.itemToPay.price) - parseInt(this.discount.amount)

      } else {
        const price = this.itemTable.filter((x) => x.group === v).map((x) => x.price).reduce((a,c) => a + c) - Math.abs(this.discount.amount)

        if (price < 0) return 0

        return price
      }
    },
    calcPrice() {
      return parseInt(this.itemToPay.price) - parseInt(this.discount.amount) /*- parseInt(this.deptAmount)*/
    },
    calcPriceGroup(v) {
      const price = this.itemTable.filter((x) => x.group === v).map((x) => x.price).reduce((a,c) => a + c) - Math.abs(this.discount.amount)

      if (price < 0) return 0

      return price /* - parseInt(this.deptAmount)*/
    },
    calcPriceGroupConv(v) {
      const price = this.itemTable.filter((x) => x.group === v).map((x) => x.convPrice).reduce((a,c) => a + c)

      if (price < 0) return 0

      return price /* - parseInt(this.deptAmount)*/
    },
    printReceipt(id , statusPayment , data , discount = 0) {
      const studyArray = []

      if (data.group) {
        this.itemTable.forEach( (it) => {
          if (it.group === data.group) {
            studyArray.push({
              statusPayment : statusPayment,
              exam : it.examType + ' ' + it.modality,
              price : it.price ,
              discount : (it.discount) ? it.discount : discount ,
              product : (it.product) ? it.product : undefined

            })
          }
        })
      } else {

        studyArray.push({
          statusPayment : statusPayment,
          exam : data.examType + ' ' + data.modality,
          price : data.price,
          discount : (data.discount) ? data.discount : discount ,
          product : (data.product) ? data.product : undefined

        })

      }

      printInvoice(this.$risReportServer, {
        studyId: data.group ? data.group : id,
        paidAmount: data.statusPayment === 'paid' ? parseFloat(data.price) : 0,
        printerIp: this.getUserInfo().setting.printerIp,
        printerPort: 9100
      }).then().catch(() => this.showError('Erreur d\'Impression'))
      /*printReceipt(id , {
        ip : this.getUserInfo().setting.printerIp,
        name : data.client,
        age : this.calcAgeFrench(new Date(data.clientObj.birthdate)),
        study : studyArray
      },this.getToken()).then(() => {
        this.showPayDialog = false
      })*/

    },
    printLargeReceipt(id , statusPayment , data , discount = 0) {
      const studyArray = []

      if (data.group) {
        this.itemTable.forEach( (it) => {
          if (it.group === data.group) {
            studyArray.push({
              id: data.id,
              statusPayment : statusPayment,
              exam : it.examType + ' ' + it.modality,
              price : it.price ,
              discount : (it.discount) ? it.discount : discount ,
              product : (it.product) ? it.product : undefined

            })
          }
        })
      } else {

        studyArray.push({
          id: data.id,
          statusPayment : statusPayment,
          exam : data.examType + ' ' + data.modality,
          price : data.price,
          discount : (data.discount) ? data.discount : discount ,
          product : (data.product) ? data.product : undefined

        })

      }

      printLargeReceipt(id , {
        ip : this.getUserInfo().setting.printerIp,
        name : data.client.client,
        age : this.calcAgeFrench(new Date(data.clientObj.birthdate)),
        client: data.client.clientObj,
        study : studyArray
      },this.getToken()).then(() => {
        this.showPayDialog = false
      })

    },
    updatePaymentStudy(v) {
      let discount = 0

      this.showPayDialog = false

      if (this.itemToPay.group) {
        let len = 0

        this.itemTable.forEach( (it) => {
          if (it.group === this.itemToPay.group) {
            len++
          }
        })
        discount = (this.discount.amount > 0) ? this.discount.amount / len : undefined

        updateGroup({
          group : this.itemToPay.group,
          discount : discount ,
          statusPayment : 'paid',
          paidBy: this.getUserInfo().id
        }, this.getToken()).then((res) => {

          this.printLargeReceipt(this.itemToPay.id ,'Payé' , this.itemToPay , discount)
          printInvoice(this.$risReportServer, {
            studyId: this.itemToPay.group ? this.itemToPay.group : this.itemToPay.id,
            paidAmount: parseFloat(this.deptAmount),
            printerIp: this.getUserInfo().setting.printerIp,
            printerPort: 9100
          }).then().catch(() => this.showError('Erreur d\'Impression'))
          this.refresh()
        })
      } else {
        discount = (this.discount.amount > 0) ? this.discount.amount : undefined

        updateStudy(this.itemToPay.id,{
          statusPayment : v ,
          discount : discount,
          paidBy: this.getUserInfo().id

        }, this.getToken())
          .then((res) => {
            this.printLargeReceipt(this.itemToPay.id ,'Payé' , this.itemToPay , discount)
            printInvoice(this.$risReportServer, {
              studyId: this.itemToPay.group ? this.itemToPay.group : this.itemToPay.id,
              paidAmount: parseFloat(this.deptAmount),
              printerIp: this.getUserInfo().setting.printerIp,
              printerPort: 9100
            }).then().catch(() => this.showError('Erreur d\'Impression'))
            this.refresh()
          })
      }
    },
    updateStudy(item,v) {
      this.itemToPay = item
      updateStudy(this.itemToPay.id,{
        statusStudy : v
      }, this.getToken()).then(() => {
        this.$router.push('/studies/')
      })
    },
    updateStudyDebt() {
      this.showPayDialog = false

      let discount  =  (this.discount.amount > 0) ? this.discount.amount : undefined
      const debtInfo = { clientid : this.itemToPay.clientObj.id ,
        createdBy : this.getUserInfo().id ,
        studyid : parseInt(this.itemToPay.id),
        discount : discount,
        paidBy: this.getUserInfo().id,
        debtAmount : this.calcPrice() ,
        debtPayed : this.deptAmount }

      if (this.itemToPay.group) {
        let len = 0

        this.itemTable.forEach( (it) => {
          if (it.group === this.itemToPay.group) {
            len++
          }
        })
        discount =  (this.discount.amount > 0) ? this.discount.amount / len : undefined
        debtInfo.debtAmount = this.calcPriceGroup(this.itemToPay.group)
        debtInfo.groupid = this.itemToPay.group,
        debtInfo.discount = discount

      }

      updateStudyDebt(debtInfo,this.getToken()).then(async (res) => {

        await this.printLargeReceipt(this.itemToPay.id,'Dette' , this.itemToPay , discount)
        printInvoice(this.$risReportServer, {
          studyId: this.itemToPay.group ? this.itemToPay.group : this.itemToPay.id,
          paidAmount: parseFloat(this.deptAmount),
          printerIp: this.getUserInfo().setting.printerIp,
          printerPort: 9100
        }).then().catch(() => this.showError('Erreur d\'Impression'))
        /**await printReceiptDebt(this.itemToPay.clientObj.id, {
          paid: this.deptAmount,
          ip: this.getUserInfo().setting.printerIp
        },this.getToken()).then((res) => {
          this.showSuccess('Success')
          this.$router.push('/studies/')
        })*/
        this.refresh()
      }).catch( (e) => {
        this.showError(e)
      })
    },
    openPayDialog(v) {

      this.itemToPay = v

      this.discount.token = {
        correct: false,
        show: false,
        value: ''
      }
      this.discount.enabled = false
      this.dialog.token = false
      this.discount.amount = 0

      this.deptAmount = '0'
      this.showPayDialog = true

    },
    refresh() {
      this.getStudies('?limit=500&sort=-createdAt')
    },
    getStudies(v) {
      this.isloadingTable = true
      this.itemTable = []
      getAllStudy(this.getToken(), v).then((res) => {
        res.data.data.forEach((it) => {
          this.itemTable.push({
            ...it,
            client: it.client.firstName + ' ' + it.client.familyName,
            clientObj: it.client
          })
        })
        this.itemTable = [...new Set(this.itemTable)]
        this.showSuccess('Success')
        this.isloadingTable = false

      }).catch((err) => {
        this.isloadingTable = false

        this.showError(err)
      })
    },
    stringToColour(str) {
      let hash = 0

      for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash)
      }
      let colour = '#'

      for (let i = 0; i < 3; i++) {
        const value = (hash >> (i * 8)) & 0xFF

        colour += ('00' + value.toString(16)).substr(-2)
      }

      return colour
    },
    getClassChip(statusStudy) {
      if (statusStudy === 'new') {
        return 'blue white--text'
      }
      if (statusStudy === 'inProgress') {
        return 'orange black--text'
      }
      if (statusStudy === 'complete') {
        return 'green white--text'
      }
      if (statusStudy === 'delivered') {
        return 'purple white--text'
      }

      return 'error white--text'

    },
    getAllModality() {
      getAllModality(this.getToken()).then((res) => {
        this.autoCompleteModalityItems = res.data.data
      }).catch((err) => {
        this.showError(err)
      })
    },
    updateConventionList() {
      this.autoCompleteConvItems = this.autoCompleteTypeExam.prices
    },
    getAllProduct() {
      getAllProduct(this.getToken(),'').then((res) => {
        this.productList = res.data.data.filter((v) => v.enabled)
      })
    },
    changeProduct() {
      let addtolist = true

      this.selectItemsProduct.forEach((it) => {
        if (it.name === this.productSelected.name) {
          addtolist = false
        }
      })

      if (addtolist) {
        this.selectItemsProduct.push({
          id : this.productSelected.id ,
          name : this.productSelected.name ,
          price : this.productSelected.sellingPrice ,
          quantityP : 1
        })

        this.productSelected = {}
      }
    },
    getAllTypeExam() {
      getAllExamTypeFilter(this.getToken() , '?modality=' + this.autoCompleteModality.id).then( (res) => {
        if (res.data.results > 0) {
          this.autoCompleteTypeExamDisable = false
          this.autoCompleteTypeExamItems = res.data.data
        }
      }).catch( (err) => {
        this.showError(err)
      })
    },
    createStudy() {
      this.studyInfo =   {
        client: this.client.id,
        modality: this.autoCompleteModality.name,
        examType: this.autoCompleteTypeExam.name,
        statusPayment: 'notPaid',
        product : this.selectItemsProduct,
        statusStudy: 'new',
        doctor: this.autoCompleteDoctor.toUpperCase(),
        price: this.autoCompleteTypeExam.price + this.calcListProductPrice(),
        createdBy: this.getUserInfo().id
      }
      if (!this.checkAutoComplete) {
        createDoctor({
          name: this.autoCompleteDoctor
        }, this.getToken())
      }

      if (this.checkConvAutoComplete) {
        this.studyInfo.price = this.autoCompleteConv.clientPrice + this.calcListProductPrice()
        this.studyInfo.convPrice = this.autoCompleteConv.companyPrice
        this.studyInfo.conv = this.autoCompleteConv.convention.name
      }
      this.showCreateDialog = false

      createStudy(this.studyInfo , this.getToken()).then( (res) => {
        this.itemTable.push({
          ...res.data.data,
          client: res.data.data.client.firstName + ' ' + res.data.data.client.familyName,
          clientObj: res.data.data.client
        })
        this.showSuccess('Success')
      }).catch((err) => {
        this.showError(err)
      })

    },
    openAddManyList() {
      this.groupDialogStudy.show = !this.groupDialogStudy.show
      const now = new Date()

      this.groupId = now.getFullYear().toString() +  (now.getMonth < 9 ? '0' : '') + now.getMonth().toString() + ((now.getDate < 10) ? '0' : '') + now.getDate().toString() + now.getHours().toString() +  now.getMinutes().toString() +  now.getSeconds().toString() +  now.getMilliseconds().toString()

    },
    addStudyToList() {
      this.studyInfo =   {
        client: this.client.id,
        modality: this.autoCompleteModality.name,
        examType: this.autoCompleteTypeExam.name,
        statusPayment: 'notPaid',
        statusStudy: 'new',
        product : this.selectItemsProduct,
        doctor: this.autoCompleteDoctor.toUpperCase(),
        price: this.autoCompleteTypeExam.price ,
        group: this.groupId,
        createdBy: this.getUserInfo().id
      }
      if (this.checkConvAutoComplete) {
        this.studyInfo.price = this.autoCompleteConv.clientPrice
        this.studyInfo.convPrice = this.autoCompleteConv.companyPrice
        this.studyInfo.conv = this.autoCompleteConv.convention.name
      }
      this.studyInfo.price += this.calcListProductPrice()

      this.groupDialogStudy.studyList.push(this.studyInfo)

      this.autoCompleteModality = ''
      this.autoCompleteTypeExam = ''
      this.checkConvAutoComplete = false
      //
      this.selectItemsProduct = []

    } ,
    creatManyStudies() {

      createStudies(this.groupDialogStudy.studyList, this.getToken()).then( (res) => {
        this.showCreateDialog = false

        // res.data.data.forEach( (item) => {
        //   this.itemTable.push( {
        //     ...item,
        //     client: item.client.firstName + ' ' + item.client.familyName,
        //     clientObj: item.client
        //   } )
        // })
        this.refresh()
        this.showSuccess('Success')
      }).catch( (err) => {
        this.showCreateDialog = false

        this.showError(err)
      })
    },
    itemRowBackground(item) {

      if (item.group) {
        return this.stringToColour(item.group.toString())
      } else {
        return '#ffffff ;'

      }
    },
    checkItem(item) {

      if ((this.itemTable.indexOf(item) ) > 0 && this.itemTable[this.itemTable.indexOf(item)].group) {
        return this.itemTable[this.itemTable.indexOf(item)].group === this.itemTable[this.itemTable.indexOf(item) - 1].group
      } else {
        return false
      }
    },
    checkDiscount() {
      if (!this.discount.amount) {
        this.discount.amount = 0
      }
    },
    verifyToken() {
      this.loading.verify = true
      getAll2fa(this.getToken(),'').then((res) => {
        verify2faToken(res.data.data[0].id, this.discount.token.value, this.getToken()).then((verify) => {
          this.discount.enabled = verify.data.data.correct
          this.discount.token.correct = verify.data.data.correct
          this.discount.token.show = true
          if (verify.data.data.correct) {
            this.dialog.token = false
            this.showSuccess(this.$t('common.success'))
          }
        }).catch((err) => {
          this.showError(err)
        })
      }).catch((err) => {
        this.showError(err)
      })

      this.loading.verify = false
    },
    cancelStudies(ids) {
      cancelStudies({ ids : ids.map((x) => parseFloat(x.id)) }, this.getToken()).then((res) => {
        this.showSuccess(this.$t('common.success'))
        this.selectedItem = []
        this.refresh()
      }).catch(() => {
        this.selectedItem = []
      })
    }
  }
}
</script>

<style >

.hadj {
  background-color: #efefef;
}

</style>
