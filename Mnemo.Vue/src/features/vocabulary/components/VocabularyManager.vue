<script setup lang="ts">
import { ref } from 'vue'
import VocabularyBar from './VocabularyBar.vue'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'
import type { SelectorItem } from '@/shared/components/ItemSelector/SelectorItem.ts'
import type { VocabularyHeader } from '../types/VocabularyHeader.ts'
import VocabularyEntryList from './VocabularyEntryList.vue'
import type { VocabularyRange } from '../types/VocabularySector.ts'
import VocabularyRangeTabs from './VocabularyRangeTabs.vue'

const selectedHeader = ref<SelectorItem<VocabularyHeader> | null>(null)
const selectedRange = ref<VocabularyRange | null>(null)
const searchQuery = ref<string>('')

async function onSearchSubmit(query: string) {
  const trimmed = query.trim()
  if (!trimmed) {
    return
  }
}

async function onCreateButton() {
  // const toggleValue =
  //   templateEntry.value === undefined
  //     ? {
  //         id: -Date.now(),
  //         partOfSpeech: undefined,
  //         foreign: '',
  //         transcription: undefined,
  //         transcriptionAudioUrl: undefined,
  //         translations: [],
  //         examples: [],
  //         synonyms: [],
  //         antonyms: [],
  //         createdAt: '',
  //       }
  //     : undefined
  // templateEntry.value = toggleValue
}
</script>

<template>
  <div class="vocabulary-container">
    <CollapsibleSection title="Vocabulary">
      <template #subtitle> Add entry </template>

      <VocabularyBar
        v-model:selected="selectedHeader"
        v-model:search-query="searchQuery"
        @submit-search="onSearchSubmit"
        @click-create="onCreateButton"
      />

      <VocabularyRangeTabs
        v-model:letter-range="selectedRange"
        :header="selectedHeader?.value ?? null"
      />

      <!-- <VocabularyItem v-if="templateEntry" :entry="templateEntry" @create="onEntryCreate" /> -->

      <VocabularyEntryList :header="selectedHeader?.value ?? null" :letter-range="selectedRange" />
    </CollapsibleSection>
  </div>
</template>

<style lang="scss" scoped>
.vocabulary-container {
  min-height: 100vh;
}
</style>
