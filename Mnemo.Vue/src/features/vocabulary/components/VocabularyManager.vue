<script setup lang="ts">
import { ref } from 'vue'
import VocabularyBar from './VocabularyBar.vue'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'
import type { SelectorItem } from '@/shared/components/ItemSelector/SelectorItem.ts'
import type { VocabularyHeader } from '../types/VocabularyHeader.ts'
import VocabularyEntryList from './VocabularyEntryList.vue'
import type { VocabularyRange } from '../types/VocabularySector.ts'
import VocabularyRangeTabs from './VocabularyRangeTabs.vue'
import type { CreateEntryRequest, VocabularyEntry } from '../types/VocabularyEntry.ts'
import { useVocabularyEntryStore } from '../stores/VocabularyEntryStore.ts'
import VocabularyItem from './VocabularyItem/VocabularyItem.vue'

const entryStore = useVocabularyEntryStore()

const selectedHeader = ref<SelectorItem<VocabularyHeader> | null>(null)
const selectedRange = ref<VocabularyRange | null>(null)
const searchQuery = ref<string>('')

function onSearchSubmit(query: string) {
  searchQuery.value = query
}

const templateEntry = ref<VocabularyEntry | undefined>(undefined)

async function onCreateButton() {
  const toggleValue =
    templateEntry.value === undefined
      ? {
          id: -Date.now(),
          linkCount: 0,
          partOfSpeech: undefined,
          foreign: '',
          transcription: undefined,
          transcriptionAudioUrl: undefined,
          translations: [],
          examples: [],
          synonyms: [],
          antonyms: [],
          createdAt: '',
        }
      : undefined
  templateEntry.value = toggleValue
}

async function onEntryCreate(bodyRequest: CreateEntryRequest) {
  templateEntry.value = undefined

  await entryStore.createEntry(selectedHeader.value?.value.guid ?? '', bodyRequest)
}
</script>

<template>
  <div class="manager-container">
    <CollapsibleSection title="Vocabulary">
      <template #subtitle>
        <button class="create-button" @click.stop="onCreateButton">
          <span class="icon">add</span>
          <span class="label">Add entry</span>
        </button>
      </template>

      <VocabularyBar
        v-model:selected="selectedHeader"
        :search-query="searchQuery"
        @submit-search="onSearchSubmit"
      />

      <VocabularyRangeTabs
        v-model:letter-range="selectedRange"
        :header="selectedHeader?.value ?? null"
      />

      <VocabularyItem
        v-if="templateEntry"
        style="margin-bottom: 15px"
        :entry="templateEntry"
        @create="onEntryCreate"
      />

      <VocabularyEntryList
        :header="selectedHeader?.value ?? null"
        :letter-range="selectedRange"
        :search-query="searchQuery"
      />
    </CollapsibleSection>
  </div>
</template>

<style lang="scss" scoped>
.manager-container {
  min-height: 100vh;

  .create-button {
    display: flex;
    align-items: center;

    margin: 0px;

    background-color: transparent;

    padding: 0px;

    .icon {
      @include iconize(19px);

      margin-right: 3px;
    }

    .label {
      color: $icon-color;

      font-size: 15px;
    }
  }
}
</style>
