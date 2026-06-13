<script setup lang="ts">
import { computed, ref } from 'vue'
import type {
  VocabularyEntry,
  VocabularyCreateRequest,
  VocabularyPatchRequest,
} from '../../types/VocabularyEntry'
import EditableField from './EditableField.vue'
import EditableList from './EditableList.vue'
import EditableSelect from './EditableSelect.vue'
import { PART_OF_SPEECH_OPTIONS } from '@/shared/constants/PartOfSpeech.ts'
import { useAudioStore } from '../../stores/AudioStore.ts'

const audio = useAudioStore()

const props = defineProps<{
  entry: VocabularyEntry
}>()

const emits = defineEmits<{
  (e: 'create', bodyRequest: VocabularyCreateRequest): void
  (e: 'delete', id: number): void
  (e: 'patch', id: number, bodyRequest: VocabularyPatchRequest): void
}>()

const isTemplateMode: boolean = props.entry.id < 0
const isEditorMode = ref<boolean>(isTemplateMode)
const isChanged = computed(
  () =>
    isTemplateMode ||
    partOfSpeechInput.value !== '' ||
    (foreignInput.value !== '' && foreignInput.value !== props.entry.foreign) ||
    (transcriptionInput.value !== '' && transcriptionInput.value !== props.entry.transcription) ||
    addTranslations.value.length > 0 ||
    removeTranslations.value.length > 0 ||
    addExamples.value.length > 0 ||
    removeExamples.value.length > 0,
)

const partOfSpeechInput = ref<string>('')
const foreignInput = ref<string>('')
const transcriptionInput = ref<string>('')

const addTranslations = ref<string[]>([])
const removeTranslations = ref<string[]>([])

const addExamples = ref<string[]>([])
const removeExamples = ref<string[]>([])

function handleExamplesUpdate(added: string[], removed: string[]) {
  addExamples.value = added
  removeExamples.value = removed
}

function handleTranslationsUpdate(added: string[], removed: string[]) {
  addTranslations.value = added
  removeTranslations.value = removed
}

function switchEditing() {
  const canSwitch =
    !isTemplateMode || (foreignInput.value.length > 0 && addTranslations.value.length > 0)

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

function emitDelete() {
  emits('delete', props.entry.id)
}

function saveChanges() {
  foreignInput.value = foreignInput.value.trim()
  transcriptionInput.value = transcriptionInput.value.trim()

  if (isTemplateMode) {
    emits('create', {
      partOfSpeech: partOfSpeechInput.value,
      foreign: foreignInput.value,
      transcription: transcriptionInput.value,
      examples: addExamples.value,
      translations: addTranslations.value,
    })
  } else {
    emits('patch', props.entry.id, {
      partOfSpeech: partOfSpeechInput.value,
      foreign: foreignInput.value,
      transcription: transcriptionInput.value,
      examplesAdd: addExamples.value,
      examplesRemove: removeExamples.value,
      translationsAdd: addTranslations.value,
      translationsRemove: removeTranslations.value,
    })
  }
}
</script>

<template>
  <article
    class="entry"
    :class="{ 'editor-mode': isEditorMode }"
    @click.left="switchEditing"
    @click.right.prevent="emitDelete"
  >
    <header>
      <EditableField
        class="foreign"
        v-model="foreignInput"
        placeholder="foreign"
        :prev-value="entry.foreign"
        :is-editor-mode="isEditorMode"
      />

      <div class="transcription">
        <span class="speech-container">
          <EditableField
            v-model="transcriptionInput"
            placeholder="[transcription]"
            :prev-value="entry.transcription"
            :is-editor-mode="isEditorMode"
          />
          <button
            v-if="entry.transcriptionAudioUrl && !isEditorMode"
            type="button"
            class="audio-button"
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
        v-model="partOfSpeechInput"
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
    grid-template-columns: 28% 29% auto auto;
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
