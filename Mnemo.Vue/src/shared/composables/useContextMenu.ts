import type { ContextMenuOption } from '@/features/contextMenu/types/ContextMenuOption'
import { ref } from 'vue'
import { useActiveInput } from '@/shared/composables/useActiveInput.ts'
import { useSelection } from '@/shared/composables/useSelection.ts'
import { useEventListener } from '@vueuse/core'

const MENU_WIDTH = 220
const MENU_ELEMENT_OFFSET = 6
const MENU_MOUSE_OFFSET = 12
const MENU_PADDING = 10
const MENU_ITEM_HEIGHT = 30
const MENU_FADE_DELAY = 120

const menuX = ref<number>(0)
const menuY = ref<number>(0)
const isVisible = ref<boolean>(false)
const hasTriangle = ref<boolean>(true)
const isLeftAligned = ref<boolean>(false)
const isTopAligned = ref<boolean>(false)

const menuOptions = ref<ContextMenuOption[]>([])
const menuDetails = ref<string[]>([])

function getMenuHeight(items: ContextMenuOption[], details: string[]) {
  return MENU_PADDING + (items.length + details.length) * MENU_ITEM_HEIGHT
}

export function useContextMenu() {
  const { hasActiveInput } = useActiveInput()
  const { hasSelection } = useSelection()

  async function openContextByElement(
    element: HTMLElement | null,
    items: ContextMenuOption[],
    details: string[] = [],
  ) {
    if (!element) return

    const rect = element.getBoundingClientRect()

    const mouseEvent = new MouseEvent('click', {
      bubbles: true,
      cancelable: true,
      clientX: rect.left,
      clientY: rect.bottom + MENU_ELEMENT_OFFSET,
    })

    await openContextByMouse(mouseEvent, items, details, false)
  }

  async function openContextByMouse(
    event: MouseEvent,
    items: ContextMenuOption[],
    details: string[] = [],
    needTriangle: boolean = true,
  ) {
    if (hasActiveInput.value || hasSelection.value) return
    event.preventDefault()
    event.stopPropagation()

    const wasOpened = isVisible.value
    isVisible.value = false
    await setTimeout(() => (isVisible.value = true), wasOpened ? MENU_FADE_DELAY : 0)

    const MENU_HEIGHT = getMenuHeight(items, details)

    const windowW = window.innerWidth
    const windowH = window.innerHeight
    const scrollX = window.pageXOffset
    const scrollY = window.pageYOffset

    const horizOffset = needTriangle ? MENU_MOUSE_OFFSET : 0
    const isFitsRight = event.clientX + MENU_WIDTH + horizOffset <= windowW
    const isFitsBottom = event.clientY + MENU_HEIGHT <= windowH

    const x = event.pageX + (isFitsRight ? horizOffset : -MENU_WIDTH - horizOffset)
    const y = event.pageY + (isFitsBottom ? 0 : -MENU_HEIGHT)

    const minX = scrollX
    const maxX = scrollX + windowW - MENU_WIDTH
    const minY = scrollY
    const maxY = scrollY + windowH - MENU_HEIGHT

    menuX.value = Math.max(minX, Math.min(maxX, x))
    menuY.value = Math.max(minY, Math.min(maxY, y))

    hasTriangle.value = needTriangle
    isLeftAligned.value = !isFitsRight
    isTopAligned.value = !isFitsBottom

    menuOptions.value = items
    menuDetails.value = details
  }

  function closeMenu() {
    if (isVisible.value) {
      isVisible.value = false
      menuOptions.value = []
      menuDetails.value = []
    }
  }

  useEventListener(window, 'click', handleOutsideClick)
  useEventListener(window, 'contextmenu', handleOutsideClick)
  useEventListener(window, 'keydown', handleEscape)
  useEventListener(window, 'resize', closeMenu)
  useEventListener(window, 'scroll', closeMenu)

  function handleOutsideClick(event: MouseEvent) {
    const menu = document.querySelector('.context-menu')
    if (menu && !menu.contains(event.target as Node)) closeMenu()
  }

  function handleEscape(event: KeyboardEvent) {
    if (event.key === 'Escape') closeMenu()
  }

  return {
    menuX,
    menuY,
    menuOptions,
    menuDetails,
    isVisible,
    hasTriangle,
    isLeftAligned,
    isTopAligned,
    openContextByMouse,
    openContextByElement,
    closeMenu,
  }
}
