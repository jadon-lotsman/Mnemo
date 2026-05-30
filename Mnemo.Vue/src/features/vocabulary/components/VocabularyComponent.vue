<script setup lang="ts">
import { computed, ref } from 'vue'
import { onMounted } from 'vue'
import { useVocabularyStore } from '../stores/VocabularyStore.ts'
import VocabularyItem from './VocabularyItem/VocabularyItem.vue'
import VocabularyToolbar from './VocabularyToolbar.vue'
import type {
  VocabularyCreateRequest,
  VocabularyEntry,
  VocabularyPatchRequest,
} from '../types/VocabularyEntry.ts'
import ItemSkeleton from './VocabularyItem/ItemSkeleton.vue'

const vocabulary = useVocabularyStore()

const templateEntry = ref<VocabularyEntry>()
const searched = ref<VocabularyEntry[]>([])
const entries = computed(() => (searched.value.length > 0 ? searched.value : vocabulary.entries))

async function onSearchSubmit(query: string) {
  const trimmed = query.trim()
  if (!trimmed) {
    searched.value = []
    return
  }

  searched.value = await vocabulary.searchVocabulary(trimmed)
}

async function onCreateButton() {
  const toggleValue =
    templateEntry.value === undefined
      ? {
          id: -Date.now(),
          foreign: '',
          transcription: '',
          translations: [],
          examples: [],
        }
      : undefined
  templateEntry.value = toggleValue
}

async function onEntryCreate(bodyRequest: VocabularyCreateRequest) {
  templateEntry.value = undefined

  await vocabulary.addEntry(bodyRequest)
}

async function onEntryPatch(id: number, bodyRequest: VocabularyPatchRequest) {
  await vocabulary.patchEntry(id, bodyRequest)
}

async function onEntryDelete(id: number) {
  await vocabulary.deleteEntry(id)
}

onMounted(async () => {
  if (vocabulary.entries.length === 0) {
    await vocabulary.fetchVocabulary()
  }
})
</script>

<template>
  <VocabularyToolbar
    :is-loading="vocabulary.isLoading"
    @submit-search="onSearchSubmit"
    @click-create="onCreateButton"
  />
  <VocabularyItem v-if="templateEntry" :entry="templateEntry" @create="onEntryCreate" />

  <div v-if="vocabulary.isLoading">
    <ItemSkeleton v-for="e in 4" :key="e" />
  </div>
  <div v-else>
    <VocabularyItem
      v-for="entry in entries"
      :key="entry.id"
      :entry="entry"
      @create="onEntryCreate"
      @patch="onEntryPatch"
      @delete="onEntryDelete"
    />
  </div>
</template>

<style lang="scss" scoped></style>
