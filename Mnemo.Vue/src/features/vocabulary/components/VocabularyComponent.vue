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
    await vocabulary.fetchVocabulary(0)
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
    <ItemSkeleton v-for="e in Math.max(4, entries.length)" :key="e" />
  </div>
  <div v-else>
    <div v-show="searched.length === 0" class="nav-bar">
      <button class="tablet-button">
        <span>sort</span>
      </button>

      <button
        class="tablet-radio"
        v-for="index in vocabulary.totalPages"
        :key="index"
        @click="vocabulary.fetchVocabulary(index)"
      >
        {{ index }}
      </button>
    </div>

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
</template>

<style lang="scss" scoped>
.nav-bar {
  display: flex;
  justify-content: end;
  flex-wrap: wrap;

  gap: 8px;

  margin-bottom: 15px;

  .tablet-button,
  .tablet-radio {
    color: $shadow;
    background-color: $plane-white;

    padding: 3px 9px 3px 9px;

    min-width: 35px;
    height: 26px;
  }

  .tablet-button {
    position: relative;

    background-color: $plane-gray;

    width: 45px;

    margin-right: auto;

    flex-shrink: 0;
    flex-grow: 0;

    span {
      @include iconize-text;

      position: absolute;

      top: 2px;
      left: 9px;
    }
  }
}
</style>
