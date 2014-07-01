-- VERSION 0.1

USE $MAIN$;

-- Initializing openId data model ... \ 

/*****************************************************************************/

-- Adding extended entity types for twitter news ... \
CALL add_type($ID$, 'Terradue.Twitter.TwitterNews, Terradue.Twitter', 'Terradue.Portal.Article, Terradue.Portal', 'Tweet', 'Tweets', 'tweet');
-- RESULT

/*****************************************************************************/

INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('Twitter-consumerKey', 'string', 'Twitter Application Consumer Key', 'Enter the value of the Twitter consumer key', 'zURZoLKwL1kW8l1v7ujkxDF8A', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('Twitter-consumerSecret', 'string', 'Twitter Application Consumer Secret', 'Enter the value of the Twitter consumer secret', '6KtHzvZRQlp2JBZ8Y2fL8dzKl39APmQllHUBtKNjTDBloamTEN', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('Twitter-token', 'string', 'Twitter Application Token', 'Enter the value of the Twitter token', '1196754241-iuj7ZgIqZwk2YpsrWC9fLnmnUH6CjA4f5M9i6hI', '0');
INSERT INTO config (`name`, `type`, `caption`, `hint`, `value`, `optional`) VALUES ('Twitter-tokenSecret', 'string', 'Twitter Application Token Secret', 'Enter the value of the Twitter token secret', 'aB92cfpkONwXOToA04ykA1Dnd6zP2Ui67y2CbkLI9mQ3R', '0');

/*****************************************************************************/
