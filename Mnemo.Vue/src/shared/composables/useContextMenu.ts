import type { ContextMenuItem } from '@/features/contextMenu/types/ContextMenuItem'
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useActiveInput } from '@/shared/composables/useActiveInput.ts'
import { useSelection } from '@/shared/composables/useSelection.ts'

export function useContextMenu() {
  const isOpen = ref(false)
  const x = ref(0)
  const y = ref(0)
  const items = ref<ContextMenuItem[]>([])
  const descriptions = ref<string[]>([])

  const inputChecker = useActiveInput()
  const selectionChecker = useSelection()

  const MENU_WIDTH = 230
  const MENU_ITEM_HEIGHT = 40

  async function open(event: MouseEvent, menuItems: ContextMenuItem[], menuDescriptions: string[]) {
    if (inputChecker.hasActiveInput.value || selectionChecker.hasSelection.value) return

    event.preventDefault()
    event.stopPropagation()

    isOpen.value = false
    await nextTick()

    items.value = menuItems
    descriptions.value = menuDescriptions
    isOpen.value = true

    const cursorX = event.clientX
    const cursorY = event.clientY
    const maxX = window.innerWidth - MENU_WIDTH
    const maxY = window.innerHeight - (menuItems.length * MENU_ITEM_HEIGHT + 20)

    x.value = Math.min(cursorX, maxX)
    y.value = Math.min(cursorY, maxY)
  }

  async function close() {
    isOpen.value = false
    items.value = []
    descriptions.value = []
  }

  function handleGlobalClick(event: MouseEvent) {
    if (!isOpen.value) return

    const menuElement = document.querySelector('.context-menu')
    if (menuElement && !menuElement.contains(event.target as Node)) {
      event.preventDefault()
      close()
    }
  }

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape' && isOpen.value) {
      close()
    }
  }

  onMounted(() => {
    window.addEventListener('click', handleGlobalClick)
    window.addEventListener('scroll', close)
    window.addEventListener('contextmenu', handleGlobalClick)
    window.addEventListener('keydown', handleKeydown)
  })

  onUnmounted(() => {
    window.removeEventListener('click', handleGlobalClick)
    window.removeEventListener('scroll', close)
    window.removeEventListener('contextmenu', handleGlobalClick)
    window.removeEventListener('keydown', handleKeydown)
  })

  return {
    isOpen,
    x,
    y,
    items,
    descriptions,
    open,
    close,
  }
}
