<script setup lang="ts">
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'
import Calendar from '@/features/calendar/components/CalendarComponent.vue'
import Launcher from '@/features/launcher/components/LauncherComponent.vue'
import Vocabulary from '@/features/vocabulary/components/VocabularyManager.vue'
import { useRouter } from 'vue-router'
import { ROUTE_NAMES } from '@/shared/constants/RouteConst'

window.scrollTo(0, 0)

const router = useRouter()

const logout = () => {
  localStorage.removeItem('token')
  router.push({ name: ROUTE_NAMES.LOGIN })
}
</script>

<template>
  <CollapsibleSection title="Calendar">
    <Calendar></Calendar>
  </CollapsibleSection>

  <CollapsibleSection title="Launcher">
    <Launcher></Launcher>
  </CollapsibleSection>

  <Vocabulary />

  <button @click="logout">logout</button>
</template>

<style lang="scss" scoped>
button {
  @include iconize(19px);
  @include lift();

  position: fixed;
  top: 5px;
  left: 5px;
  background-color: $elevated-bg;

  color: $shadow-color;
}
</style>
