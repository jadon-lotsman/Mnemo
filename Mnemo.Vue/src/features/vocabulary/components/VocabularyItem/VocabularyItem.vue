<script setup lang="ts">
import { computed, ref } from 'vue'
import type {
  VocabularyEntry,
  CreateEntryRequest,
  PatchEntryRequest,
} from '../../types/VocabularyEntry'
import EditableField from './EditableField.vue'
import EditableList from './EditableList.vue'
import EditableSelect from './EditableSelect.vue'
import { PART_OF_SPEECH_OPTIONS } from '@/shared/constants/PartOfSpeech.ts'
import { useAudioStore } from '../../stores/AudioStore.ts'
import { useSelection } from '@/shared/composables/useSelection.ts'

const audio = useAudioStore()

const selectionChecker = useSelection()

const props = defineProps<{
  entry: VocabularyEntry
}>()

const emits = defineEmits<{
  (e: 'create', bodyRequest: CreateEntryRequest): void
  (e: 'patch', id: number, bodyRequest: PatchEntryRequest): void
}>()

const audioTitle = computed(() => {
  const parts = props.entry.transcriptionAudioUrl?.split('/')
  return parts != null ? parts[parts.length - 1] : null
})

const isTemplateMode: boolean = props.entry.id < 0
const isEditorMode = ref<boolean>(isTemplateMode)

const inputPartOfSpeech = ref<string>('')
const inputForeign = ref<string>('')
const inputTranscription = ref<string>('')

const addTranslations = ref<string[]>([])
const removeTranslations = ref<string[]>([])
const addExamples = ref<string[]>([])
const removeExamples = ref<string[]>([])

const isChanged = computed(
  () =>
    isTemplateMode ||
    (inputPartOfSpeech.value !== '' && inputPartOfSpeech.value != props.entry.partOfSpeech) ||
    (inputForeign.value !== '' && inputForeign.value !== props.entry.foreign) ||
    (inputTranscription.value !== '' && inputTranscription.value !== props.entry.transcription) ||
    addTranslations.value.length > 0 ||
    removeTranslations.value.length > 0 ||
    addExamples.value.length > 0 ||
    removeExamples.value.length > 0,
)

function handleExamplesUpdate(added: string[], removed: string[]) {
  addExamples.value = added
  removeExamples.value = removed
}

function handleTranslationsUpdate(added: string[], removed: string[]) {
  addTranslations.value = added
  removeTranslations.value = removed
}

function switchEditing() {
  if (selectionChecker.hasSelection.value) return

  const canSwitch =
    !isTemplateMode || (inputForeign.value.length > 0 && addTranslations.value.length > 0)

  if (canSwitch) isEditorMode.value = !isEditorMode.value

  if (!isEditorMode.value && isChanged.value) {
    saveChanges()
  }
}

function toggleAudio(url: string) {
  if (!url) return
  if (audio.isPlayingThis(url)) audio.stop()
  else audio.play(url)
}

function saveChanges() {
  const finalForeign = inputForeign.value.trim()
  const finalTranscription = inputTranscription.value.trim()
  const finalPartOfSpeech = inputPartOfSpeech.value.trim()

  if (isTemplateMode) {
    emits('create', {
      partOfSpeech: finalPartOfSpeech || undefined,
      foreign: finalForeign,
      transcription: finalTranscription || undefined,
      examples: addExamples.value,
      translations: addTranslations.value,
    })
    return
  }

  const patchRequest: PatchEntryRequest = {}

  if (finalPartOfSpeech !== '' && finalPartOfSpeech !== props.entry.partOfSpeech) {
    patchRequest.partOfSpeech = finalPartOfSpeech
  }
  if (finalForeign !== '' && finalForeign !== props.entry.foreign) {
    patchRequest.foreign = finalForeign
  }
  if (finalTranscription != '' && finalTranscription !== props.entry.transcription) {
    patchRequest.transcription = finalTranscription
  }
  if (addExamples.value.length > 0) {
    patchRequest.examplesAdd = addExamples.value
  }
  if (removeExamples.value.length > 0) {
    patchRequest.examplesRemove = removeExamples.value
  }
  if (addTranslations.value.length > 0) {
    patchRequest.translationsAdd = addTranslations.value
  }
  if (removeTranslations.value.length > 0) {
    patchRequest.translationsRemove = removeTranslations.value
  }

  if (Object.keys(patchRequest).length === 0) return

  emits('patch', props.entry.id, patchRequest)
}
</script>

<template>
  <article class="entry" :class="{ 'editor-mode': isEditorMode }" @click="switchEditing">
    <header>
      <EditableField
        class="foreign"
        v-model="inputForeign"
        placeholder="foreign"
        :prev-value="entry.foreign"
        :is-editor-mode="isEditorMode"
      />

      <div class="transcription">
        <span class="speech-container">
          <EditableField
            v-model="inputTranscription"
            placeholder="[transcription]"
            :prev-value="entry.transcription"
            :is-editor-mode="isEditorMode"
          />
          <button
            v-if="entry.transcriptionAudioUrl && !isEditorMode"
            type="button"
            class="audio-button"
            :title="audioTitle ?? ''"
            @click.stop="toggleAudio(entry.transcriptionAudioUrl)"
          >
            {{ audio.isPlayingThis(entry.transcriptionAudioUrl) ? 'volume_up' : 'volume_down' }}
          </button>
        </span>
      </div>

      <EditableList
        class="translations"
        placeholder="Input a new translation..."
        :exist-items="entry.translations"
        :is-editor-mode="isEditorMode"
        @update="handleTranslationsUpdate"
      />

      <EditableSelect
        class="part-of-speech"
        v-model="inputPartOfSpeech"
        :prev-value="entry.partOfSpeech"
        :options="PART_OF_SPEECH_OPTIONS"
        :is-editor-mode="isEditorMode"
      />
    </header>

    <footer>
      <EditableList
        placeholder="Input a new example..."
        :exist-items="entry.examples"
        :is-editor-mode="isEditorMode"
        :capitalize-items="true"
        :mark-items="true"
        @update="handleExamplesUpdate"
      />
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
    grid-template-columns: 30% 30% auto auto;
    background-color: $plane-white;

    padding: 10px 15px;

    border-radius: 12px;

    .foreign {
      grid-column: 1;
    }

    .transcription {
      grid-column: 2;

      .speech-container {
        display: inline-flex;
        justify-content: start;
        align-items: start;
        flex-wrap: nowrap;

        margin-right: 4px;
        gap: 2px;

        .audio-button {
          @include iconize-text;

          color: $shadow;
          background-color: inherit;

          box-shadow: none;

          padding: 0px;

          opacity: 65%;

          line-height: 0.8;
          font-size: 24px;
        }
      }
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

    .part-of-speech {
      grid-column: 4;
    }
  }

  &.editor-mode header {
    grid-template-columns: auto 40% auto;

    .foreign {
      grid-column: 1;
      grid-row: 1;
    }

    .transcription {
      grid-column: 1;
      grid-row: 1;

      margin: 0px;
      margin-top: 28px;
      margin-bottom: 5px;

      .speech-container {
        flex-direction: row-reverse;
      }
    }

    .translations {
      grid-column: 2;
      grid-row: 1/3;
    }

    .part-of-speech {
      grid-column: 3;
      grid-row: 1;

      justify-self: end;
    }
  }

  footer {
    padding: 15px;

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
}
</style>
