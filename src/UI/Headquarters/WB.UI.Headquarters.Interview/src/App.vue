<template>
    <div>
        <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
            <div class="container-fluid ">
                <!-- Brand and toggle get grouped for better mobile display -->
                <div class="navbar-header">
                    <a href="#" class="active-page">Interview</a>
                    <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar top-menu"></span>
                        <span class="icon-bar mid-menu"></span>
                        <span class="icon-bar bottom-menu"></span>
                    </button>
                    <a class="navbar-brand  rotate-brand" href="#">
                        <div class="brand-name" title="Headquarter">Web interview</div>
                    </a>
                </div>
                <!-- Collect the nav links, forms, and other content for toggling -->
                <div class="collapse navbar-collapse" id="navbar">
                    <ul class="nav navbar-nav navbar-right">
                        <li><a href="#" title="Help">Help</a></li>
                    </ul>
                </div>
                <!-- /.navbar-collapse -->
            </div>
            <!-- /.container-fluid -->
        </nav>
        <main class="web-interview hold-transition">
            <div class="container-fluid">
                <div class="row">
                    <section class="questionnaire">
                        <router-view></router-view>
                    </section>
                </div>
            </div>
        </main>
    </div>
</template>
<script lang='ts'>
    export default {
        name: 'app'
    }
</script>
<style lang='scss'>
  @import "assets/styles"
</style>
