<script setup lang="ts">
import { computed, ref } from 'vue'
import { capitalize } from '@/shared/utils/StringExtension'
import type {
  VocabularyEntry,
  VocabularyCreateRequest,
  VocabularyPatchRequest,
} from '../types/VocabularyEntry'

const props = defineProps<{
  entry: VocabularyEntry
}>()

const emits = defineEmits<{
  (e: 'create', localId: number, bodyRequest: VocabularyCreateRequest): void
  (e: 'patch', remoteId: number, bodyRequest: VocabularyPatchRequest): void
}>()

const isCreatingMode: boolean = props.entry.id < 0
const isEditorMode = ref<boolean>(isCreatingMode)

const foreignInput = ref<string>('')
const transcriptionInput = ref<string>('')

const translationsInput = ref<string>('')
const addTranslations = ref<string[]>([])
const removeTranslations = ref<string[]>([])
const displayTranslations = computed(() => {
  const all = [...props.entry.translations, ...addTranslations.value]
  return all.filter((translation) => !removeTranslations.value.includes(translation))
})

const exampleInput = ref<string>('')
const addExamples = ref<string[]>([])
const removeExamples = ref<string[]>([])
const displayExamples = computed(() => {
  const all = [...props.entry.examples, ...addExamples.value]
  return all.filter((example) => !removeExamples.value.includes(example))
})

function pushItem(mode: string) {
  if (mode === 'examples') {
    addExamples.value.push(exampleInput.value)
    exampleInput.value = ''
  } else {
    addTranslations.value.push(translationsInput.value)
    translationsInput.value = ''
  }
}

function removeItem(str: string, mode: string) {
  if (mode === 'examples') {
    if (addExamples.value.includes(str)) {
      addExamples.value = addExamples.value.filter((example) => example !== str)
    } else {
      removeExamples.value.push(str)
    }
  } else {
    if (addTranslations.value.includes(str)) {
      addTranslations.value = addTranslations.value.filter((translation) => translation !== str)
    } else {
      removeTranslations.value.push(str)
    }
  }
}

function switchEditing() {
  isEditorMode.value = !isEditorMode.value

  if (isEditorMode.value === false) {
    saveChanges()
  }
}

function saveChanges() {
  if (isCreatingMode) {
    emits('create', props.entry.id, {
      foreign: foreignInput.value,
      transcription: transcriptionInput.value,
      examples: addExamples.value,
      translations: addTranslations.value,
    })
  } else {
    emits('patch', props.entry.id, {
      foreign: foreignInput.value,
      transcription: transcriptionInput.value,
      examplesAdd: addExamples.value,
      examplesRemove: removeExamples.value,
      translationsAdd: addTranslations.value,
      translationsRemove: removeTranslations.value,
    })
  }

  foreignInput.value = ''
  transcriptionInput.value = ''

  addExamples.value = []
  removeExamples.value = []
  addTranslations.value = []
  removeTranslations.value = []
}
</script>

<template>
  <article class="entry" :class="{ 'editor-mode': isEditorMode }" @click="switchEditing">
    <header>
      <input
        class="foreign"
        type="text"
        v-if="isEditorMode"
        :placeholder="entry.foreign === '' ? 'foreign' : entry.foreign"
        v-model="foreignInput"
        @click.stop
      />
      <div class="foreign" v-else>{{ entry.foreign }}</div>

      <input
        class="transcription"
        type="text"
        v-if="isEditorMode"
        :placeholder="entry.transcription === '' ? 'transcription' : entry.transcription"
        v-model="transcriptionInput"
        @click.stop
      />
      <div class="transcription" v-else>{{ entry.transcription }}</div>

      <ol class="translations">
        <div class="editable-item" v-for="translation in displayTranslations" :key="translation">
          <button v-if="isEditorMode" @click.stop="removeItem(translation, 'translations')">
            close
          </button>
          <li>
            {{ translation }}
          </li>
        </div>
      </ol>
      <form
        class="add-form"
        v-if="isEditorMode"
        @submit.prevent="pushItem('translations')"
        @click.stop
      >
        <input type="text" placeholder="Input a new translation..." v-model="translationsInput" />
        <button type="button" @click.stop>add</button>
      </form>
    </header>

    <footer>
      <ol v-if="displayExamples.length > 0">
        <div class="editable-item" v-for="example in displayExamples" :key="example">
          <button v-if="isEditorMode" @click.stop="removeItem(example, 'examples')">close</button>
          <li>
            {{ capitalize(example) }}
          </li>
        </div>
      </ol>
      <form class="add-form" v-if="isEditorMode" @submit.prevent="pushItem('examples')" @click.stop>
        <input type="text" placeholder="Input a new example..." v-model="exampleInput" />
        <button type="submit">add</button>
      </form>
    </footer>
  </article>
</template>

<style lang="scss" scoped>
.entry {
  cursor: default;

  display: flex;
  flex-direction: column;
  align-items: stretch;

  transition:
    transform 0.2s,
    box-shadow 0.2s ease;

  color: $black-font;
  background-color: $plane-gray;
  box-shadow: 5px 5px 0px $shadow;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  font-size: 16px;

  &.editor-mode {
    box-shadow: 8px 8px 0px $shadow;
    transform: translateY(-3px);
  }

  header {
    display: grid;
    grid-template-columns: 30% 30% 40%;
    background-color: $plane-white;

    padding: 10px 15px;

    border-radius: 12px;

    .foreign {
      grid-column: 1;
    }

    .transcription {
      grid-column: 2;

      margin: 0 5px;
    }

    input.foreign,
    input.transcription {
      color: $black-font;
      background-color: transparent;

      height: 20px;
      width: 80%;

      font-size: 16px;
    }

    .translations {
      grid-column: 3;

      display: flex;
      flex-direction: column;

      margin-top: -2px;

      li {
        display: flex;
        flex-direction: row;
        justify-content: space-between;
      }
    }
  }

  &.editor-mode header {
    grid-template-columns: 50% 50%;

    .foreign {
      grid-column: 1;
      grid-row: 1;
    }

    .transcription {
      grid-column: 1;
      grid-row: 1;

      margin: 0px;
      margin-top: 28px;
    }

    .translations {
      grid-column: 2;
      grid-row: 1/3;
    }
  }

  footer {
    padding: 15px;

    li {
      font-size: 16px;
      font-style: italic;

      &::before {
        content: '–';
        padding-right: 10px;
      }
    }

    &:empty {
      padding: 10px;
    }
  }

  &.editor-mode footer {
    padding-bottom: 5px;
    padding-top: 5px;

    ol {
      margin-top: 10px;
    }
  }

  .editable-item {
    display: flex;
    justify-content: start;
    align-items: center;

    button {
      @include iconize-text;

      color: $shadow;
      background-color: transparent;

      box-shadow: none;

      padding: 0px;
      margin-left: -5px;
      margin-right: 5px;

      font-size: 21px;
    }
  }

  .add-form {
    display: flex;
    justify-content: space-between;
    align-items: center;

    grid-column: 1/4;

    color: $gray-font;
    background-color: transparent;

    input {
      background-color: inherit;

      width: 100%;

      font-size: 16px;
      font-style: italic;
    }

    button {
      @include iconize-text;

      color: $shadow;
      background-color: inherit;

      box-shadow: none;

      padding: 4px;
    }

    &::before {
      content: '–';
      padding-right: 10px;
    }
  }
}
</style>
