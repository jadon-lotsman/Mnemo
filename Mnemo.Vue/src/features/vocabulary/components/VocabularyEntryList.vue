<script setup lang="ts">
import { ref, watch } from 'vue'
import type { VocabularyHeader } from '../types/VocabularyHeader'
import type { VocabularyRange } from '../types/VocabularySector'
import type { PatchEntryRequest, VocabularyEntry } from '../types/VocabularyEntry'
import { useVocabularyEntryStore } from '../stores/VocabularyEntryStore'
import { useContextMenu } from '@/shared/composables/useContextMenu'
import ItemSkeleton from './VocabularyItem/ItemSkeleton.vue'
import VocabularyItem from './VocabularyItem/VocabularyItem.vue'
import { useInfiniteScroll } from '@vueuse/core'
import { format } from 'date-fns'

const props = defineProps<{
  header: VocabularyHeader | null
  letterRange: VocabularyRange | null
}>()

const entryStore = useVocabularyEntryStore()
const { openContextByMouse } = useContextMenu()

const autoscrollRef = ref<HTMLElement | null>(null)

const currentPage = ref<number>(1)

useInfiniteScroll(
  autoscrollRef,
  async () => {
    if (
      props.letterRange == null ||
      currentPage.value >= entryStore.totalPages ||
      entryStore.loadingPlaceholder.isLoading
    )
      return

    currentPage.value++
    await entryStore.fetchPage(
      props.header?.guid ?? '',
      props.letterRange?.startWord ?? '',
      props.letterRange?.endWord ?? '',
      currentPage.value,
    )
  },
  { distance: 100 },
)

async function openContextMenu(event: MouseEvent, entry: VocabularyEntry) {
  if (entry.id < 0) return

  const creatingDate = format(new Date(entry.createdAt), 'd MMM, yyyy')

  await openContextByMouse(event, {
    modules: [
      {
        type: 'list',
        options: [
          {
            label: 'Delete entry',
            icon: 'close',
            action: () => onEntryDelete(entry.id),
          },
        ],
      },
      {
        type: 'text',
        text: `Created at ${creatingDate}`,
      },
    ],
  })
}

async function onEntryPatch(id: number, bodyRequest: PatchEntryRequest) {
  await entryStore.patchEntry(props.header?.guid ?? null, id, bodyRequest)
}

async function onEntryDelete(id: number) {
  await entryStore.deleteEntry(props.header?.guid ?? null, id)
}

async function reloadPage() {
  currentPage.value = 1

  if (props.letterRange === null) return

  await entryStore.fetchPage(
    props.header?.guid ?? null,
    props.letterRange?.startWord ?? '',
    props.letterRange?.endWord ?? '',
    currentPage.value,
  )
}

watch(
  () => props.header,
  async () => {
    await entryStore.resetPages()
    await reloadPage()
  },
)

watch(
  () => props.letterRange,
  async () => {
    await reloadPage()
  },
)
</script>

<template>
  <div v-if="entryStore.loadingPlaceholder.showSkeleton" class="list-container">
    <ItemSkeleton v-for="i in 5" :key="i" />
  </div>
  <div v-else class="list-container">
    <VocabularyItem
      v-for="entry in entryStore.entries"
      :key="entry.id"
      :entry="entry"
      @patch="onEntryPatch"
      @contextmenu.capture="(e: MouseEvent) => openContextMenu(e, entry)"
    />

    <span ref="autoscrollRef" class="more-placeholder" v-if="currentPage < entryStore.totalPages"
      >Loading entries...
    </span>
  </div>
</template>

<style lang="scss" scoped>
.list-container {
  display: flex;
  flex-direction: column;
  justify-content: start;

  gap: 15px;

  padding-bottom: 10vh;

  .more-placeholder {
    display: block;

    padding-top: 5px;
    padding-bottom: 30px;

    color: $text-secondary;

    text-align: center;
  }
}
</style>
