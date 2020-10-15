#!/bin/sh

# Constants
UPM_ROOT="Assets"
IGNORES=""
THIS_SCRIPT=`basename $0`
TARGET_DIR="Runtime"
BRANCH_NAME="upm"

create_tag()
{
	local tags=$(git tag)
	for t in $tags
	do
		echo $t
		local n="${t##*.}"
		echo $n
		echo `expr $n + 1`
	done
}

create_branch()
{
	local branch=$(git branch | grep $1)
	if [[ $branch == $1 ]] 
	then
    	git branch -D $1
	fi
	git subtree split --prefix=$UPM_ROOT --branch $1
	git checkout $1
}

should_move()
{
	if [[ `basename $1` == $THIS_SCRIPT ]]
	then
		return 0
	fi

	if [[ `basename $1` == $TARGET_DIR ]]
	then
		return 0
	fi

    if [[ -d $1 ]]
    then
        return 1
    else
        return 1
    fi
}

reorganize_files()
{
	if [ -e $TARGET_DIR ]
	then
		rm -r $TARGET_DIR
	fi

	mkdir $TARGET_DIR

	for f in ./*
	do
	    should_move $f
        if [ $? -ne 0 ]
        then
			mv $f $TARGET_DIR
        fi
	done
}

main()
{
	create_branch $BRANCH_NAME

	# reorganize_files

	create_tag
}

main
echo Press any key to continue
read -n 1
