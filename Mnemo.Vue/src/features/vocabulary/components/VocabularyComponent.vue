<script setup lang="ts">
import { computed, ref } from 'vue'
import { onMounted } from 'vue'
import { useVocabularyStore } from '../stores/VocabularyStore.ts'
import VocabularyItem from './VocabularyItem/VocabularyItem.vue'
import VocabularyToolbar from './VocabularyToolbar.vue'
import type {
  CreateEntryRequest,
  VocabularyEntry,
  PatchEntryRequest,
} from '../types/VocabularyEntry.ts'
import ItemSkeleton from './VocabularyItem/ItemSkeleton.vue'
import { useNotify } from '@/shared/composables/useNotify.ts'
import { useContextMenu } from '@/shared/composables/useContextMenu.ts'
import ContextMenu from '@/features/contextMenu/components/ContextMenu.vue'
import type { ContextMenuItem } from '@/features/contextMenu/types/ContextMenuItem.ts'
import { format } from 'date-fns'
import VocabularyNavbar from './VocabularyNavbar.vue'

const notify = useNotify()
const vocabulary = useVocabularyStore()

const contextMenu = useContextMenu()

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

  if (searched.value.length == 0) notify.info(`No matches for '${query}'`)
}

async function onSortSubmit(isDescending: boolean) {
  await vocabulary.fetchSectors(isDescending)

  const firstPage = vocabulary.sectors[0]
  if (firstPage !== undefined) {
    await onPageSubmit(firstPage.startWord, firstPage.endWord)
  }
}

async function onPageSubmit(startWord: string, endWord: string) {
  vocabulary.fetchPage(startWord, endWord)
}

async function onCreateButton() {
  const toggleValue =
    templateEntry.value === undefined
      ? {
          id: -Date.now(),
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

  await vocabulary.addEntry(bodyRequest)
}

async function onEntryPatch(id: number, bodyRequest: PatchEntryRequest) {
  await vocabulary.patchEntry(id, bodyRequest)
}

async function onEntryDelete(id: number) {
  await vocabulary.deleteEntry(id)
}

async function openContextMenu(event: MouseEvent, entry: VocabularyEntry) {
  if (entry.id < 0) return

  const menuItems: ContextMenuItem[] = [
    // {
    //   label: 'Pin/Unpin',
    //   icon: 'keep',
    //   action: () => console.log('pinned'),
    // },
    // {
    //   label: 'Reset progress',
    //   icon: 'replay',
    //   action: () => console.log('reset'),
    // },
    {
      label: 'Delete entry',
      icon: 'close',
      action: () => onEntryDelete(entry.id),
    },
  ]

  const creatingDate = format(new Date(entry.createdAt), 'd MMM, yyyy')
  const menuDescriptions: string[] = [`Created at ${creatingDate}`]

  contextMenu.open(event, menuItems, menuDescriptions)
}

onMounted(async () => {
  if (vocabulary.entries.length === 0) {
    onSortSubmit(false)
  }
})
</script>

<template>
  <div class="vocabulary-container">
    <VocabularyToolbar
      :is-loading="vocabulary.loadingPlaceholder.showSkeleton"
      @submit-search="onSearchSubmit"
      @click-create="onCreateButton"
    />

    <VocabularyNavbar
      :is-loading="vocabulary.loadingPlaceholder.showSkeleton"
      :tablets="vocabulary.sectors"
      :disable-tablets="searched.length > 0"
      @refresh-sort="onSortSubmit"
      @submit-page="onPageSubmit"
    />

    <VocabularyItem v-if="templateEntry" :entry="templateEntry" @create="onEntryCreate" />

    <div v-if="vocabulary.loadingPlaceholder.showSkeleton">
      <ItemSkeleton v-for="e in Math.max(4, entries.length)" :key="e" />
    </div>
    <div v-else>
      <VocabularyItem
        v-for="entry in entries"
        :key="entry.id"
        :entry="entry"
        @create="onEntryCreate"
        @patch="onEntryPatch"
        @contextmenu.capture="(e: MouseEvent) => openContextMenu(e, entry)"
      />
    </div>

    <ContextMenu
      :is-open="contextMenu.isOpen.value"
      :x="contextMenu.x.value"
      :y="contextMenu.y.value"
      :items="contextMenu.items.value"
      :descriptions="contextMenu.descriptions.value"
      @close="contextMenu.close"
    />
  </div>
</template>

<style lang="scss" scoped>
.vocabulary-container {
  min-height: 100vh;
}
</style>
