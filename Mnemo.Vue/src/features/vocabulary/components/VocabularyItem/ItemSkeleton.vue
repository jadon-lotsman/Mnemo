<template>
  <article class="entry-skeelton">
    <header>
      <div class="some-text"></div>

      <div class="some-text"></div>

      <div class="list">
        <div class="some-text"></div>
        <div class="some-text"></div>
        <div class="some-text"></div>
      </div>
    </header>

    <footer>
      <div class="list"></div>
    </footer>
  </article>
</template>

<style lang="scss" scoped>
@keyframes pulse {
  0% {
    opacity: 0.5;
  }
  50% {
    opacity: 0.8;
  }
  100% {
    opacity: 0.5;
  }
}

.entry-skeelton {
  cursor: default;

  display: flex;
  flex-direction: column;
  align-items: stretch;

  background-color: $plane-gray;
  box-shadow: 5px 5px 0px $shadow;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  .some-text {
    height: 16px;
    width: 80%;
    background: $plane-gray;
    margin-bottom: 4px;
    border-radius: 4px;
    animation: pulse 1.5s infinite ease-in-out;
  }

  .list {
    display: flex;
    flex-direction: column;

    margin-top: -2px;
  }

  header {
    display: grid;
    grid-template-columns: 30% 30% 40%;
    background-color: $plane-white;

    padding: 10px 15px;

    border-radius: 12px;
  }

  footer {
    padding: 20px 15px;
  }
}
</style>
