
IMAGE = pullenti/pullenti-server

EP.SdkCore:
	cp -r ../PullentiNetCore/EP.SdkCore .

image: EP.SdkCore
	docker build -t $(IMAGE) .

push:
	docker push pullenti/pullenti-server

deamon:
	docker run -d -p 8080:8080 $(IMAGE)

run:
	docker run -it --rm -p 8080:8080 $(IMAGE)
