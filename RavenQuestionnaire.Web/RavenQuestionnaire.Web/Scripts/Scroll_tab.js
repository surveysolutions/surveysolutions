$(document).ready(function () {
    var $win = $('#content'), current = 0, current_top = 0, threashold = 0;

    //class="group-header" scroll_fixed="0" scroll_top="1" scroll_top_index="1">

    $('.group-header').each(function () {

        $(this).attr('scroll_offset_top', $(this).offset().top);
        $(this).attr('scroll_fixed', "0");
    });
    $('.display-group').each(function () {
        if ($(this).parent().parent().attr('id') == 'content') {
            current_top++;
            $(this).find('div.group-header:first').attr('scroll_top', '1');
            $(this).find('div.group-header').attr('scroll_top_index', current_top);
        }
    });
    // $(window).scroll(processScroll());
    current_top = 0;
    $('#content').scroll(function () {
        $nav = $('.group-header');
        $nav.each(function () {
            navTop = $(this).attr('scroll_offset_top') * 1 - 60;
            var i, scrollTop = $('#content').scrollTop();
            var isFixed = $(this).attr('scroll_fixed') * 1;

            if (scrollTop + (current * 60) > navTop && !isFixed) {
                isFixed = 1;


                if ($(this).attr('scroll_top') == '1') {
                    $nav.removeClass('subnav-fixed');
                    current = 1;
                    current_top = $(this).attr('scroll_top_index') * 1;
                    threashold = 0;

                } else current++;

                $(this).addClass('subnav-fixed');
                if (current > 1) $(this).css('top', current * 49);


            } else if (scrollTop + (current * 60) <= navTop && isFixed) {
                isFixed = 0;
                if ($(this).attr('scroll_top') == '1' && current_top > 1) {
                    current_top--;
                    //$('[scroll_top_index=' + current_top + ']').addClass('subnav-fixed');
                    current = 0;
                    $('[scroll_top_index=' + current_top + ']').each(function () {
                        $(this).addClass('subnav-fixed');
                        current++;
                        $(this).attr('scroll_fixed', isFixed);
                        threashold = 150;

                    });

                } else current--;
                $(this).removeClass('subnav-fixed');


            }
            $(this).attr('scroll_fixed', isFixed);

        });

    });

})