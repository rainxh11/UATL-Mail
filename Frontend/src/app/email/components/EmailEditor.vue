<template>
  <div
    class="email-editor pa-2"
    @drop.prevent="onDrop($event)"
    @dragover.prevent="dragover = true"
    @dragenter.prevent="dragover = true"
    @dragleave.prevent="dragover = false"
    @dragstart.prevent="dragover = true"
  >

    <email-input :label="$t('email.to') + ':'" :addresses.sync="toAddresses">
    </email-input>
    <v-divider class="pa-1"></v-divider>

    <v-text-field
      v-model="subject"
      :label="$t('email.subject')"
      outlined
      flat
      hide-details
      clearable
      clear-icon="fa-circle-xmark"
    ></v-text-field>
    <v-divider></v-divider>

    <editor-menu-bar v-slot="{ commands, isActive }" :editor="editor">
      <div class="pa-1">
        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.bold() }"
          @click="commands.bold"
        >
          <v-icon>mdi-format-bold</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.italic() }"
          @click="commands.italic"
        >
          <v-icon>mdi-format-italic</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.strike() }"
          @click="commands.strike"
        >
          <v-icon>mdi-format-strikethrough</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.underline() }"
          @click="commands.underline"
        >
          <v-icon>mdi-format-underline</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.paragraph() }"
          @click="commands.paragraph"
        >
          <v-icon>mdi-format-paragraph</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.heading({ level: 1 }) }"
          @click="commands.heading({ level: 1 })"
        >
          H1
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.heading({ level: 2 }) }"
          @click="commands.heading({ level: 2 })"
        >
          H2
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.heading({ level: 3 }) }"
          @click="commands.heading({ level: 3 })"
        >
          H3
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.bullet_list() }"
          @click="commands.bullet_list"
        >
          <v-icon>mdi-format-list-bulleted</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.ordered_list() }"
          @click="commands.ordered_list"
        >
          <v-icon>mdi-format-list-numbered</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.blockquote() }"
          @click="commands.blockquote"
        >
          <v-icon>mdi-format-quote-close</v-icon>
        </v-btn>

        <v-btn
          icon
          tile
          :class="{ 'is-active': isActive.code_block() }"
          @click="commands.code_block"
        >
          <v-icon>mdi-code-tags</v-icon>
        </v-btn>

        <v-btn icon tile @click="commands.horizontal_rule">
          <v-icon>mdi-minus</v-icon>
        </v-btn>

        <v-btn icon tile @click="commands.undo">
          <v-icon>mdi-undo</v-icon>
        </v-btn>

        <v-btn icon tile @click="commands.redo">
          <v-icon>mdi-redo</v-icon>
        </v-btn>

      </div>
    </editor-menu-bar>

    <v-divider></v-divider>

    <editor-content class="editor__content pa-3 py-4" :editor="editor" />

    <v-divider></v-divider>
    <v-card-text
      v-if="dragover"
      :class="[ dragover ? 'grey lighten-2' : '']"
    >
      <v-row
        class="d-flex flex-column"
        dense
        align="center"
        justify="center"
      >
        <v-icon color="info" class="" size="60">
          fa-solid fa-cloud-arrow-up
        </v-icon>
        <p>
          {{ $t('email.uploadHint') }}
        </p>
      </v-row>
      <v-virtual-scroll
        v-if="files.length > 0"
        :items="fileList"
        item-height="50"
        min-height="100"
        max-height="300"
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

    <v-divider/>
    <div class="d-flex align-center pa-2">
      <v-btn :disabled="toAddresses.length === 0" color="primary" @click="$emit('refresh')">
        <v-icon small class="pa-1">fa-solid fa-paper-plane-top</v-icon>
        {{ $t('email.send') }}
      </v-btn>
      <v-file-input
        v-model="tempFiles"
        class="d-flex align-center pa-2"
        hide-input
        append-icon="fa-solid fa-paperclip"
        multiple
      />
      <v-spacer/>
      <v-btn color="info" :loading="saveLoading" @click="saveDraft()">
        <v-icon small class="pa-1">fa-solid fa-save</v-icon>
        {{ $t('email.saveDraft') }}
      </v-btn>
    </div>
  </div>
</template>

<script>
import EmailInput from './EmailInput'
import { Editor, EditorContent, EditorMenuBar } from 'tiptap'
import {
  Blockquote,
  CodeBlock,
  HardBreak,
  Heading,
  HorizontalRule,
  OrderedList,
  BulletList,
  ListItem,
  TodoItem,
  TodoList,
  Bold,
  Code,
  Italic,
  Link,
  Strike,
  Underline,
  History
} from 'tiptap-extensions'
import { Html5Entities } from 'html-entities'
import { mapActions, mapGetters } from 'vuex'
import { createDraft } from '@/api/drafts'

export default {
  components: {
    EditorContent,
    EditorMenuBar,
    EmailInput
  },
  data() {
    return {
      subject: '',
      body:'',
      uploadDialog: false,
      toAddresses: [],
      recipients: [],
      toggleFormat: [],
      saveLoading: false,
      files: [],
      tempFiles: [],
      editor: null,
      dragover: false
    }
  },
  computed:{
    fileList() {
      return this.$enumerable(this.files).Distinct((x) => x.name + x.size).OrderByDescending((x) => x.size).ToArray()
    }
  },
  watch: {
    tempFiles: function (val) {
      if (val.length > 0) {
        this.files = this.$enumerable(this.files)
          .Concat(val)
          .ToArray()
      }
      this.dragover = val.length > 0
    }
  },
  created() {
    this.editor = new Editor({
      extensions: [
        new Blockquote(),
        new BulletList(),
        new CodeBlock(),
        new HardBreak(),
        new Heading({ levels: [1, 2, 3] }),
        new HorizontalRule(),
        new ListItem(),
        new OrderedList(),
        new TodoItem(),
        new TodoList(),
        new Link(),
        new Bold(),
        new Code(),
        new Italic(),
        new Strike(),
        new Underline(),
        new History()
      ],
      content:`
          <h3>
            Hi there,
          </h3>
          <p>
            <br />
            I'll take my boat.
          </p>
          <blockquote>
            Best regards 👏
            <br />
            – Tom Cruise
          </blockquote>
        `
    })
  },
  beforeDestroy() {
    this.editor.destroy()
  },
  methods:{
    ...mapGetters('auth', ['getToken', 'getUserInfo']),
    ...mapActions('app', ['showSuccess', 'showError']),

    removeFile(name) {
      this.files = this.$enumerable(this.files)
        .Where((x) => x.name !== name)
        .ToArray()
    },
    onDrop(e) {
      console.log(e.dataTransfer.files)
      this.files = this.$enumerable(this.files)
        .Concat(e.dataTransfer.files)
        .ToArray()
    },
    saveDraft() {
      const draft = {
        Subject: this.subject,
        Body: Html5Entities.encode(this.editor.getHTML())
      }

      this.saveLoading = true
      createDraft(draft, this.fileList, this.getToken())
        .then(() => {
          this.$emit('close')
        })
        .catch((err) => console.log(err))
        .finally(() => this.saveLoading = false)
    },
    getFiles(val) {
      this.files = val
    }
  }
}
</script>

<style lang="scss">
 .drop-area {
   position: relative;
   border: gray dotted 2px;
   min-height: 10em;
   text-align: center;
   margin-bottom: .5em;
   color: gray;
 }
.email-editor {
  position: relative;

  .v-btn {
    &.is-active {
      background-color: #f1f1f1;
    }
  }

  .editor__content {
    overflow-wrap: break-word;
    word-wrap: break-word;
    word-break: break-word;

    * {
      caret-color: currentColor;
    }

    .ProseMirror {
      &:focus {
        outline: none;
      }
    }

    ul,
    ol {
      padding-left: 1rem;
    }

    li > p,
    li > ol,
    li > ul {
      margin: 0;
    }

    a {
      color: inherit;
    }

    blockquote {
      border-left: 3px solid rgba(0, 0, 0, 0.1);
      color: rgba(0, 0, 0, 0.8);
      padding-left: 0.8rem;
      font-style: italic;

      p {
        margin: 0;
      }
    }

    img {
      max-width: 100%;
      border-radius: 3px;
    }

    table {
      border-collapse: collapse;
      table-layout: fixed;
      width: 100%;
      margin: 0;
      overflow: hidden;

      td,
      th {
        min-width: 1em;
        border: 2px solid #fafafa;
        padding: 3px 5px;
        vertical-align: top;
        box-sizing: border-box;
        position: relative;
        > * {
          margin-bottom: 0;
        }
      }

      th {
        font-weight: bold;
        text-align: left;
      }

      .selectedCell:after {
        z-index: 2;
        position: absolute;
        content: "";
        left: 0;
        right: 0;
        top: 0;
        bottom: 0;
        background: rgba(200, 200, 255, 0.4);
        pointer-events: none;
      }

      .column-resize-handle {
        position: absolute;
        right: -2px;
        top: 0;
        bottom: 0;
        width: 4px;
        z-index: 20;
        background-color: #adf;
        pointer-events: none;
      }
    }

    .tableWrapper {
      margin: 1em 0;
      overflow-x: auto;
    }

    .resize-cursor {
      cursor: ew-resize;
      cursor: col-resize;
    }
  }
}
</style>
